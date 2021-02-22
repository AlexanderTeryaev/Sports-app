using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AppModel;
using DataService;
using WebGrease.Css.Extensions;

namespace CmsApp.App_Start
{
    /// <summary>
    /// Update TeamPlayers table to add LeagueId and ClubId column values.
    /// After db is updated this code can be safely removed.
    /// 
    /// I am not that good at T-SQL as I am at .net, so I wrote this migration as code-based migration
    /// </summary>
    internal static class MigrationTeamPlayersAddLeagueIdAndClubId
    {
        /// <summary>
        /// Update TeamPlayers table to add LeagueId and ClubId column values.
        /// After db is updated this code can be safely removed.
        /// 
        /// I am not that good at T-SQL as I am at .net, so I wrote this migration as code-based migration
        /// </summary>
        internal static void Execute()
        {
            var db = new DataEntities();
            var repo = new BaseRepo(db);

            var toRemove = new List<TeamsPlayer>();
            var toAdd = new List<TeamsPlayer>();
            var toAddTrainingAttendances = new List<TrainingAttendance>();
            var lostTeamPlayers = new List<TeamsPlayer>();

            var teamPlayersToMigrate = repo.GetCollection<TeamsPlayer>(x => x.LeagueId == null && x.ClubId == null).ToList();

            foreach (var teamPlayer in teamPlayersToMigrate.ToList())
            {
                var team = teamPlayer.Team;
                var player = teamPlayer.User;

                if (team.LeagueTeams != null && team.LeagueTeams.Any(x => x.SeasonId == teamPlayer.SeasonId))
                {
                    var leagueTeams = team.LeagueTeams.Where(x => x.SeasonId == teamPlayer.SeasonId).ToList();

                    toAdd.AddRange(leagueTeams.Select(leagueTeam =>
                    {
                        var newTp = new TeamsPlayer
                        {
                            TeamId = team.TeamId,
                            UserId = player.UserId,
                            LeagueId = leagueTeam.LeagueId,
                            SeasonId = teamPlayer.SeasonId,
                            ClubComment = teamPlayer.ClubComment,
                            UnionComment = teamPlayer.UnionComment,
                            HandicapLevel = teamPlayer.HandicapLevel,
                            IsActive = teamPlayer.IsActive,
                            IsApprovedByManager = teamPlayer.IsApprovedByManager,
                            IsLocked = teamPlayer.IsLocked,
                            IsTrainerPlayer = teamPlayer.IsTrainerPlayer,
                            PosId = teamPlayer.PosId,
                            ShirtNum = teamPlayer.ShirtNum,
                            StartPlaying = teamPlayer.StartPlaying,
                        };

                        if (teamPlayer.TrainingAttendances?.Any() == true)
                        {
                            toAddTrainingAttendances.AddRange(teamPlayer.TrainingAttendances.ToList().Select(x => new TrainingAttendance
                            {
                                TeamsPlayer = newTp,
                                TrainingId = x.TrainingId
                            }));
                        }

                        return newTp;
                    }));
                }

                if (team.ClubTeams != null && team.ClubTeams.Any(x => x.SeasonId == teamPlayer.SeasonId))
                {
                    var clubTeams = team.ClubTeams.Where(x => x.SeasonId == teamPlayer.SeasonId).ToList();

                    toAdd.AddRange(clubTeams.Select(clubTeam =>
                    {
                        var newTp = new TeamsPlayer
                        {
                            TeamId = team.TeamId,
                            UserId = player.UserId,
                            ClubId = clubTeam.ClubId,
                            SeasonId = teamPlayer.SeasonId,
                            ClubComment = teamPlayer.ClubComment,
                            UnionComment = teamPlayer.UnionComment,
                            HandicapLevel = teamPlayer.HandicapLevel,
                            IsActive = teamPlayer.IsActive,
                            IsApprovedByManager = teamPlayer.IsApprovedByManager,
                            IsLocked = teamPlayer.IsLocked,
                            IsTrainerPlayer = teamPlayer.IsTrainerPlayer,
                            PosId = teamPlayer.PosId,
                            ShirtNum = teamPlayer.ShirtNum,
                            StartPlaying = teamPlayer.StartPlaying,
                        };

                        if (teamPlayer.TrainingAttendances?.Any() == true)
                        {
                            toAddTrainingAttendances.AddRange(teamPlayer.TrainingAttendances.ToList().Select(x => new TrainingAttendance
                            {
                                TeamsPlayer = newTp,
                                TrainingId = x.TrainingId
                            }));
                        }

                        return newTp;
                    }));
                }

                if (team.SchoolTeams != null && team.SchoolTeams.Any(x => x.School.SeasonId == teamPlayer.SeasonId))
                {
                    var schoolTeams = team.SchoolTeams.Where(x => x.School.SeasonId == teamPlayer.SeasonId).ToList();

                    toAdd.AddRange(schoolTeams.Select(schoolTeam =>
                    {
                        var newTp = new TeamsPlayer
                        {
                            TeamId = team.TeamId,
                            UserId = player.UserId,
                            ClubId = schoolTeam.School.Club.ClubId,
                            SeasonId = teamPlayer.SeasonId,
                            ClubComment = teamPlayer.ClubComment,
                            UnionComment = teamPlayer.UnionComment,
                            HandicapLevel = teamPlayer.HandicapLevel,
                            IsActive = teamPlayer.IsActive,
                            IsApprovedByManager = teamPlayer.IsApprovedByManager,
                            IsLocked = teamPlayer.IsLocked,
                            IsTrainerPlayer = teamPlayer.IsTrainerPlayer,
                            PosId = teamPlayer.PosId,
                            ShirtNum = teamPlayer.ShirtNum,
                            StartPlaying = teamPlayer.StartPlaying,
                        };

                        if (teamPlayer.TrainingAttendances?.Any() == true)
                        {
                            toAddTrainingAttendances.AddRange(teamPlayer.TrainingAttendances.ToList().Select(x => new TrainingAttendance
                            {
                                TeamsPlayer = newTp,
                                TrainingId = x.TrainingId
                            }));
                        }

                        return newTp;
                    }));
                }

                toRemove.Add(teamPlayer);
                lostTeamPlayers.Add(teamPlayer);
            }

            db.GamesCycles.RemoveRange(toRemove
                .Where(x => x.GamesCycles != null && x.GamesCycles.Any())
                .Select(x => x.GamesCycles)
                .Select(x => x.FirstOrDefault()));

            foreach (var playoffBracket in toRemove
                .Where(x => x.PlayoffBrackets != null && x.PlayoffBrackets.Any())
                .Select(x => x.PlayoffBrackets)
                .Select(x => x.FirstOrDefault()).ToList())
            {
                foreach (var b in playoffBracket.PlayoffBrackets1.ToList())
                {
                    db.PlayoffBrackets.Remove(b);
                }
                foreach (var b in playoffBracket.PlayoffBrackets11.ToList())
                {
                    db.PlayoffBrackets.Remove(b);
                }

                db.PlayoffBrackets.Remove(playoffBracket);
            }

            db.TrainingAttendances.RemoveRange(toRemove
                .Where(x => x.TrainingAttendances != null && x.TrainingAttendances.Any())
                .Select(x => x.TrainingAttendances)
                .Select(x => x.FirstOrDefault()));
            db.Statistics.RemoveRange(toRemove
                .Where(x => x.Statistics != null && x.Statistics.Any())
                .Select(x => x.Statistics)
                .Select(x => x.FirstOrDefault()).ToList());
            db.GroupsTeams.RemoveRange(toRemove
                .Where(x => x.GroupsTeams != null && x.GroupsTeams.Any())
                .Select(x => x.GroupsTeams)
                .Select(x => x.FirstOrDefault()));
          
            db.TeamsPlayers.RemoveRange(toRemove);
            db.TeamsPlayers.AddRange(toAdd);

            db.SaveChanges();

            db.TrainingAttendances.AddRange(toAddTrainingAttendances);

            db.SaveChanges();
        }
    }
}