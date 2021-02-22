using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AppModel;
using DataService;
using WebApi.Models;

namespace WebApi.Services
{
    public static class PlayerService
    {
        internal static List<CompactPlayerViewModel> GetActivePlayersByTeamId(int teamId, int seasonId, int leagueId)
        {
            using (var db = new DataEntities())
            {
                var repo = new BaseRepo(db);
                var section = repo.GetSectionByTeamId(teamId);
                var isWaterPolo = section?.Alias == GamesAlias.WaterPolo;
                var isNetball = section?.Alias == GamesAlias.NetBall;
                var isBasketball = section?.Alias == GamesAlias.BasketBall;
                var isTennis = section?.Alias == GamesAlias.Tennis;
                var isRugby = section?.Alias == GamesAlias.Rugby;
                var players = db.TeamsPlayers
                    .Include(p => p.User.ActivityFormsSubmittedDatas)
                    .Join(db.Users, tp => tp.UserId, user => user.UserId, (tp, user) => new {tp, user})
                    .Where(t =>
                        t.tp.TeamId == teamId &&
                        t.tp.SeasonId == seasonId &&
                        (isTennis || t.tp.LeagueId == leagueId) &&
                        t.tp.IsActive &&
                        t.user.IsActive &&
                        t.user.PenaltyForExclusions.Where(c => !c.IsCanceled).All(c => c.IsEnded) &&
                        !t.tp.IsTrainerPlayer)
                    .ToList()
                    .Select(t => new CompactPlayerViewModel
                    {
                        playerId = t.tp.Id,
                        Id = t.tp.UserId,
                        FullName = t.tp.User.FullName,
                        Height = t.tp.User.Height,
                        TennisPositionOrder = t.tp.TennisPositionOrder,
                        BirthDay = t.tp.User.BirthDay,
                        Age = DateTime.Now.Year - t.tp.User.BirthDay?.Year,
                        Image =
                            t.tp.User.PlayerFiles
                                .Where(x => x.SeasonId == seasonId && x.FileType == (int) PlayerFileType.PlayerImage)
                                .Select(x => x.FileName)
                                .FirstOrDefault() ?? t.tp.User.Image,
                        ShirtNumber = t.tp.ShirtNum,
                        PositionTitle = t.tp.Position?.Title,
                        ActivityForms = t.tp.User.ActivityFormsSubmittedDatas,
                        IsApprovedByManager = t.tp.IsApprovedByManager
                    })
                    .ToList();

                var result = new List<CompactPlayerViewModel>();
                if (isWaterPolo || isNetball || isRugby)
                {
                    foreach (var player in players)
                    {
                        var registration = player.ActivityForms?.FirstOrDefault(x =>
                            //x.Activity.IsAutomatic == true &&
                                x.Activity.Type == ActivityType.Personal &&
                                x.Activity.SeasonId == seasonId &&
                                x.TeamId == teamId &&
                                x.LeagueId == leagueId);
                        var isRegistered = player.IsApprovedByManager == true
                                           || registration?.IsActive == true;
                        if (isRegistered)
                        {
                            result.Add(player);
                        }
                    }

                    return result;
                }

                if (isBasketball)
                {
                    var playerRepo = new PlayersRepo();
                    var unionId = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId)?.UnionId;
                    foreach (var player in players)
                    {
                        var teamPlayer = playerRepo.GetTeamsPlayerById(player.playerId);

                        player.HandicapLevel = teamPlayer.HandicapLevel;

                        //player.FinalHandicapLevel = _playerRepo.GetFinalHandicap(teamPlayer, out decimal _, seasonId, unionId, teamPlayer.LeagueId);
                        //Remove the line below and uncomment the one above if calculated handicap is needed once again
                        player.FinalHandicapLevel = teamPlayer.HandicapLevel;

                        result.Add(player);
                    }
                }
                else
                {
                    foreach (var player in players)
                    {
                        /*if (isTennis)
                        {
                            if (player.BirthDay != null)
                            {
                                player.Height = DateTime.Now.Year - player.BirthDay.Value.Year;//not club
                            }
                            else
                            {
                                player.Height = 0;
                            }
                        }*/
                        result.Add(player);
                    }
                }

                return result;
            }
        }

        internal static List<CompactPlayerViewModel> GetActivePlayersByClubTeam(int teamId, int seasonId,
            int currentUserId, int clubId, bool bActive = true)
        {
            using (var db = new DataEntities())
            {
                var repo = new BaseRepo(db);
                var section = repo.GetSectionByTeamId(teamId);
                var isWaterPolo = section?.Alias == GamesAlias.WaterPolo;
                var isTennis = section?.Alias == GamesAlias.Tennis;
                var isAthletic = section?.Alias == GamesAlias.Athletics;
                var isRugby = section?.Alias == GamesAlias.Rugby;
                var players = db.TeamsPlayers.Include(p => p.User.ActivityFormsSubmittedDatas)
                    .SelectMany(tp => db.Positions.Where(res => res.PosId == tp.PosId).DefaultIfEmpty(),
                        (tp, Position) => new {tp, Position})
                    .Join(db.Users, t => t.tp.UserId, user => user.UserId, (t, user) => new {t, user})
                    .Where(t => t.t.tp.TeamId == teamId && t.t.tp.SeasonId == seasonId &&
                                /* tp.ClubId == clubId && */ // cheng remove:
                                (!bActive || t.t.tp.IsActive && t.user.IsActive && t.user.PenaltyForExclusions
                                     .Where(c => !c.IsCanceled)
                                     .All(c => c.IsEnded)))
                    .OrderBy(t => t.t.tp.UserId)
                    .ToList()
                    .Select(t => new CompactPlayerViewModel
                    {
                        playerId = t.t.tp.Id,
                        Id = t.t.tp.UserId,
                        FullName = t.t.tp.User.FullName,
                        Height = t.t.tp.User.Height,
                        BirthDay = t.t.tp.User.BirthDay,
                        Age = DateTime.Now.Year - t.t.tp.User.BirthDay?.Year,
                        Image = t.t.tp.User.PlayerFiles.Where(x =>
                                        x.SeasonId == seasonId && x.FileType == (int) PlayerFileType.PlayerImage)
                                    .Select(x => x.FileName)
                                    .FirstOrDefault() ?? t.t.tp.User.Image,
                        ShirtNumber = t.t.tp.ShirtNum,
                        PositionTitle = t.t.Position == null ? null : t.t.Position.Title,
                        ActivityForms = t.t.tp.User.ActivityFormsSubmittedDatas,
                        IsApprovedByManager = t.t.tp.IsApprovedByManager
                    })
                    .ToList();

                var result = new List<CompactPlayerViewModel>();

                if (isWaterPolo || isRugby)
                {
                    var prevId = 0;
                    foreach (var player in players)
                    {
                        var isRegistered = player.IsApprovedByManager == true
                                           || player.ActivityForms?.FirstOrDefault(x =>
                                               x.Activity.IsAutomatic == true &&
                                               x.Activity.Type == ActivityType.Personal &&
                                               x.Activity.SeasonId == seasonId) != null;
                        if (isRegistered)
                        {
                            if (prevId != player.Id)
                            {
                                player.FriendshipStatus = FriendsService.AreFriends(player.Id, currentUserId);
                                result.Add(player);
                            }
                        }

                        prevId = player.Id;
                    }

                    return result;
                }

                foreach (var player in players)
                {
                    player.FriendshipStatus = FriendsService.AreFriends(player.Id, currentUserId);
                    player.PlayerDiscipline =
                        db.PlayerDisciplines
                            .Where(x => x.ClubId == clubId && x.SeasonId == seasonId && x.PlayerId == player.Id)
                            .Select(x => x.Discipline.Name).FirstOrDefault() ?? null;
                    if (isAthletic)
                    {
                        //var discipline_Id = db.CompetitionDisciplineRegistrations.AsNoTracking().Where(r => r.UserId == player.Id &&
                        //r.CompetitionDiscipline.League.SeasonId == seasonId && !r.IsArchive && !r.CompetitionDiscipline.IsDeleted && !r.CompetitionDiscipline.League.IsArchive
                        //&& r.CompetitionResult.FirstOrDefault() != null && r.CompetitionResult.FirstOrDefault().Result.Length > 0)
                        //.OrderBy(r => r.CompetitionDiscipline.League.LeagueStartDate ?? DateTime.MaxValue).FirstOrDefault()?.CompetitionDiscipline?.DisciplineId??0;
                        //player.PlayerDiscipline = db.Disciplines.Where(x => x.DisciplineId == discipline_Id).FirstOrDefault()?.Name;
                        if (player.IsApprovedByManager != true)
                        {
                            continue;
                        }
                    }
                    if (isTennis)
                    {
                        if(player.BirthDay != null)
                        {
                            //player.Height = DateTime.Now.Year - player.BirthDay.Value.Year;
                            //player.Age = DateTime.Now.Year - player.BirthDay.Value.Year;
                        }
                    }
                    result.Add(player);
                }

                return result;
            }
        }

        internal static PlayerProfileViewModel GetPlayerProfile(User player, int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var tempInfo = db.TeamsPlayers.FirstOrDefault(x => x.UserId == player.UserId && seasonId == x.SeasonId);
                var vm = new PlayerProfileViewModel
                {
                    Id = player.UserId,
                    FullName = player.FullName,
                    Height = player.Height,
                    BirthDay = player.BirthDay,
                    Age = DateTime.Now.Year - db.Users.FirstOrDefault(x => x.UserId == player.UserId).BirthDay
                              .Value.Year,
                    ShirtN = tempInfo?.ShirtNum ?? 0,
                    Role = tempInfo != null ? tempInfo.Position != null ? tempInfo.Position.Title : "" : "",
                    Image = player.PlayerFiles.Where(x =>
                                    (seasonId == null || x.SeasonId == seasonId) &&
                                    x.FileType == (int) PlayerFileType.PlayerImage)
                                .Select(x => x.FileName)
                                .FirstOrDefault() ?? player.Image,
                    UserRole = player.UsersType == null ? "" : player.UsersType?.TypeRole
                };
                return vm;
            }
        }
        internal static PlayerProfileViewModel GetPlayerProfileClubApp(User player, int? seasonId,int? lid, int? clubId)
        {
            using (var db = new DataEntities())
            {
                var tempInfo = db.TeamsPlayers.FirstOrDefault(x => x.UserId == player.UserId && seasonId == x.SeasonId);
                if (lid.HasValue && lid != 0)
                {
                    tempInfo = db.TeamsPlayers.FirstOrDefault(x => x.UserId == player.UserId && seasonId == x.SeasonId && lid == x.LeagueId);
                }
                else if(clubId.HasValue && clubId != 0)
                {
                    tempInfo = db.TeamsPlayers.FirstOrDefault(x => x.UserId == player.UserId && seasonId == x.SeasonId && clubId == x.ClubId);
                }

                var vm = new PlayerProfileViewModel
                {
                    Id = player.UserId,
                    FullName = player.FullName,
                    Height = player.Height,
                    BirthDay = player.BirthDay,
                    Age = DateTime.Now.Year -
                          db.Users.FirstOrDefault(x => x.UserId == player.UserId).BirthDay.Value.Year,
                    ShirtN = tempInfo != null ? tempInfo.ShirtNum : 0,
                    Role = tempInfo != null ? tempInfo.Position != null ? tempInfo.Position.Title : "" : "",
                    Image = player.PlayerFiles.Where(x =>
                                    (seasonId == null || x.SeasonId == seasonId) &&
                                    x.FileType == (int) PlayerFileType.PlayerImage)
                                .Select(x => x.FileName)
                                .FirstOrDefault() ?? player.Image,
                    UserRole = player.UsersType == null ? "" : player.UsersType?.TypeRole
                };
                return vm;
            }
        }
    }
}