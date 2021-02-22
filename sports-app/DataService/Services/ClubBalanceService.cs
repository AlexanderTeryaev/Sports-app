using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;
using DataService.DTO;

namespace DataService.Services
{
    public class ClubBalanceService
    {
        private readonly DataEntities db;
        private readonly ClubsRepo clubsRepo;

        public ClubBalanceService(DataEntities db)
        {
            this.db = db;
            this.clubsRepo = new ClubsRepo(db);
        }

        public IEnumerable<ClubBalanceDto> GetClubBalance(int id, int seasonId)
        {
            var clubBalance = db.ClubBalances.AsNoTracking()
                .Where(t => t.SeasonId == seasonId && t.ClubId == id)?.ToList();
            return ProcessCurrentClubBalance(clubBalance);
        }

        public ClubBalance GetById(int id)
        {
            return db.ClubBalances.Where(t => t.Id == id).FirstOrDefault();
        }

        private IEnumerable<ClubBalanceDto> ProcessCurrentClubBalance(List<ClubBalance> clubBalances)
        {
            var currentClubBalance = 0M;

            for (int i = 0; i < clubBalances.Count; i++)
            {
                var income = clubBalances[i].Income ?? 0;
                var expense = clubBalances[i].Expense ?? 0;
                currentClubBalance = i == 0
                    ? income - expense
                    : currentClubBalance + income - expense;
                var userAction = clubBalances[i].User.UserName;
                if(String.IsNullOrEmpty(userAction))
                {
                    userAction = clubBalances[i].User.FullName;
                }


                yield return new ClubBalanceDto
                {
                    Id = clubBalances[i].Id,
                    Income = income,
                    Expense = expense,
                    Balance = currentClubBalance,
                    ActionUser = new ActionUser
                    {
                        UserId = clubBalances[i].User.UserId,
                        FullName = userAction
                    },
                    Comment = clubBalances[i].Comment,
                    SeasonId = clubBalances[i].SeasonId,
                    TimeOfAction = clubBalances[i].TimeOfAction,
                    IsPdfReport = clubBalances[i].IsPdfReport,
                    IsPaid = clubBalances[i].IsPaid,
                    Reference = clubBalances[i].Reference
                };
            }

        }

        public void CreateBalanceRecord(int id, ClubBalanceDto balance)
        {
            var model = new ClubBalance
            {
                ActionUserId = balance.ActionUser.UserId,
                ClubId = id,
                Comment = balance.Comment,
                Expense = balance.Expense,
                Income = balance.Income,
                SeasonId = balance.SeasonId,
                Reference = balance.Reference,
                TimeOfAction = DateTime.Now
            };

            clubsRepo.CreateClubBalance(model);
            clubsRepo.Save();
        }

        public ClubBalance CreateBalanceRecordForReport(int id, ClubBalanceDto balance, List<TeamsPlayer> players, List<TeamRegistration> teamsRegistrations)
        {
            var model = new ClubBalance
            {
                ActionUserId = balance.ActionUser.UserId,
                ClubId = id,
                Comment = balance.Comment,
                Expense = balance.Expense,
                Income = balance.Income,
                SeasonId = balance.SeasonId,
                TimeOfAction = DateTime.Now,
                IsPdfReport = balance.IsPdfReport,
                IsPaid = balance.IsPaid,
                LeagueId = balance.LeagueId,
            };
            var distinctPlayers = players.GroupBy(p => p.UserId).Select(g => g.First()).ToList();
            var duplications = players.Select(x => x).ToList();
            foreach (var teamPlayer in distinctPlayers)
            {
                duplications.Remove(teamPlayer);
            }
            
            var playersValidityCost = 60.0m * distinctPlayers.Count();

            foreach (var player in distinctPlayers)
            {
                model.TeamPlayersPayments.Add(new TeamPlayersPayment
                {
                    TeamPlayerId = player.Id,
                    Fee = 60.0m,
                    Validity = player.User.TenicardValidity
                });
            }
            foreach (var player in duplications)
            {
                model.TeamPlayersPayments.Add(new TeamPlayersPayment
                {
                    TeamPlayerId = player.Id,
                    Fee = 0.0m,
                    Validity = player.User.TenicardValidity
                });
            }
            decimal totalRegCost = 0.0m;
            foreach (var teamsRegistration in teamsRegistrations)
            {
                
                decimal? priceOfTeamRegistration = teamsRegistration.League.LeaguesPrices.FirstOrDefault(p => p.PriceType == 1)?.Price;
                if (priceOfTeamRegistration.HasValue)
                {
                    totalRegCost += priceOfTeamRegistration.Value;
                }
                model.TeamRegistrationPayments.Add(new TeamRegistrationPayment
                {
                    TeamRegistrationId = teamsRegistration.Id,
                    Fee = priceOfTeamRegistration
                });
            }
            model.Expense = playersValidityCost + totalRegCost;
            clubsRepo.CreateClubBalance(model);
            clubsRepo.Save();
            return model;
        }

        public void UpdateBalanceRecord(int id, ClubBalanceDto balance)
        {
            clubsRepo.UpdateClubBalance(balance);
            clubsRepo.Save();
        }

        public void DeleteBalanceRecord(int clubBalanceId)
        {
            clubsRepo.DeleteClubBalance(clubBalanceId);
            clubsRepo.Save();
        }


        public void UpdateBalancePaymentStatus(int clubBalanceId, bool isPaid)
        {
            var clubBalance = GetById(clubBalanceId);
            var season = db.Seasons.FirstOrDefault(s => s.Id == clubBalance.SeasonId);
            if (clubBalance != null)
            {
                clubBalance.IsPaid = isPaid;
                if(isPaid)
                {
                    clubBalance.Income = clubBalance.Expense;
                }
                else
                {
                    clubBalance.Income = 0.0m;
                }
                if (isPaid)
                {
                    var year = clubBalance.Season.EndDate.Year;
                    var tenicardValidityUntil = new DateTime(year, 12, 31);
                    // update tenicard to all those players.
                    foreach (var player in clubBalance.TeamPlayersPayments.Select(p => p.TeamPlayers))
                    {
                        player.User.TenicardValidity = tenicardValidityUntil;
                    }
                }
                db.SaveChanges();
            }
        }


    }
}
