using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;
using WebApi.Models;

namespace WebApi.Services
{
    public class RefereeSevice : IDisposable
    {
        DataEntities db = new DataEntities();

        public IList<GameViewModel> GetRefereeGames(int refereeId, int? seasonId)
        {
            var refereeStr = "," + refereeId + ",";
            var refereeGames = (from gameCycle in db.GamesCycles
                                join stage in db.Stages on gameCycle.StageId equals stage.StageId
                                join homeTeam in db.Teams on gameCycle.HomeTeamId equals homeTeam.TeamId
                                join guestTeam in db.Teams on gameCycle.GuestTeamId equals guestTeam.TeamId
                                join auditorium in db.Auditoriums on gameCycle.AuditoriumId equals auditorium.AuditoriumId into aud
                                from gameCycleAuditorium in aud.DefaultIfEmpty()

                                let homeTeamDetails = homeTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                let guestTeamDetails = guestTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                let lastGameSet = gameCycle.GameSets.OrderByDescending(x=>x.GameSetId).FirstOrDefault()
                                let homeTeamGameSetSum = gameCycle.GameSets.Any() ? gameCycle.GameSets.Sum(x => x.HomeTeamScore) : 0
                                let guestTeamGameSetSum = gameCycle.GameSets.Any() ? gameCycle.GameSets.Sum(x => x.GuestTeamScore) : 0
                                let alias = stage.League.Union.Section.Alias

                                where !string.IsNullOrEmpty(gameCycle.RefereeIds) &&
                                      ("," + gameCycle.RefereeIds + ",").Contains(refereeStr) &&
                                      gameCycle.IsPublished
                                where gameCycle.Group.SeasonId == seasonId
                                orderby gameCycle.StartDate
                                select new GameViewModel
                                {
                                    GameId = gameCycle.CycleId,
                                    GameCycleStatus = gameCycle.GameStatus,
                                    StartDate = gameCycle.StartDate,
                                    HomeTeamId = homeTeam.TeamId,
                                    HomeTeam = homeTeamDetails != null ? homeTeamDetails.TeamName : homeTeam.Title,
                                    HomeTeamScore = (alias == GamesAlias.WaterPolo || alias == GamesAlias.Soccer || alias == GamesAlias.BasketBall || alias == GamesAlias.Rugby || alias == GamesAlias.Softball ? homeTeamGameSetSum : gameCycle.HomeTeamScore),
                                    GuestTeam = guestTeamDetails != null ? guestTeamDetails.TeamName : guestTeam.Title,
                                    GuestTeamId = guestTeam.TeamId,
                                    GuestTeamScore = (alias == GamesAlias.WaterPolo || alias == GamesAlias.Soccer || alias == GamesAlias.BasketBall || alias == GamesAlias.Rugby || alias == GamesAlias.Softball ? guestTeamGameSetSum : gameCycle.GuestTeamScore),
                                    Auditorium = gameCycleAuditorium != null ? gameCycleAuditorium.Name : null,
                                    AuditoriumAddress = gameCycleAuditorium != null ? gameCycleAuditorium.Address : null,
                                    HomeTeamLogo = homeTeam.Logo,
                                    GuestTeamLogo = guestTeam.Logo,
                                    CycleNumber = gameCycle.CycleNum,
                                    LeagueId = stage.LeagueId,
                                    LeagueName = stage.League.Name
                                }).ToList();

            return refereeGames;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}