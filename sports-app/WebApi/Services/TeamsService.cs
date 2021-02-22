using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AppModel;
using DataService.LeagueRank;
using WebApi.Controllers;
using WebApi.Models;
using WebApi.Helpers;

namespace WebApi.Services
{
    public class TeamsService : IDisposable
    {
        #region Constructor & fields
        private readonly DataEntities _db;


        public TeamsService(DataEntities db)
        {
            _db = db;
        }
        public TeamsService()
        {
            _db = new DataEntities();
        }
        #endregion

        internal static TeamInfoViewModel GetTeamInfo(Team team, int leagueId, int? seasonId = null, bool isTennis = false)
        {
            if(!isTennis)
            {
                //if (team.LeagueTeams.All(l => l.LeagueId != leagueId))
                //{
                //    return null;
                //}
            }

            var vm = new TeamInfoViewModel();
            //teamId
            vm.TeamId = team.TeamId;
            vm.Logo = team.Logo;
            vm.Image = team.PersonnelPic;
            if (!isTennis)
                vm.League = team.LeagueTeams.FirstOrDefault(tl => tl.LeagueId == leagueId)?.Leagues?.Name;
            else
                vm.League = team.TeamRegistrations.FirstOrDefault(tl => tl.LeagueId == leagueId)?.TeamName;

            vm.LeagueId = leagueId;

            if (seasonId.HasValue)
            {
                TeamsDetails teamsDetails = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId);
                vm.Title = teamsDetails != null ? teamsDetails.TeamName : team.Title;
            }
            else
            {
                vm.Title = team.Title;
            }
            vm.Description = team.Description;

            var rLeague = CacheService.CreateLeagueRankTable(leagueId, seasonId, isTennis);

            List<RankStage> LeagueTableStages = null;

            if (rLeague != null)
            {
                LeagueTableStages = rLeague.Stages;
                LeaugesController.MakeGroupStages(LeagueTableStages, isEmpty: false);
            }

            if (LeagueTableStages == null || LeagueTableStages.Count == 0)
            {
                LeagueTableStages = CacheService.CreateEmptyRankTable(leagueId, seasonId)?.Stages;
                if(LeagueTableStages != null)
                {
                    LeaugesController.MakeGroupStages(LeagueTableStages, isEmpty: true);
                }
                else
                {
                    return vm;
                }
            }
            LeagueTableStages = LeagueTableStages.Where(x => x.Groups.All(y => !y.IsAdvanced)).ToList();
            LeagueTableStages.Reverse();
            // get team score
            if (LeagueTableStages.Count() > 0)
            {
                foreach (RankStage rs in LeagueTableStages)
                {
                    var group = rs.Groups.FirstOrDefault(gr => gr.Teams.Any(t => t.Id == team.TeamId));
                    if (group != null)
                    {
                        RankTeam rTeam = group.Teams.FirstOrDefault(t => t.Id == team.TeamId);
                        if (rTeam != null)
                        {
                            if (!isTennis)
                            {
                                vm.Place = rTeam.PositionNumber;
                                vm.Ratio = rTeam.SetsLost + ":" + rTeam.SetsWon;

                                if (rTeam.Games == 0)
                                {
                                    vm.SuccsessLevel = 0;
                                }
                                else
                                {
                                    double wins = rTeam.Wins;
                                    double games = rTeam.Games;
                                    double ratio = (wins / games) * 100;
                                    vm.SuccsessLevel = Convert.ToInt32(ratio);
                                }
                            }
                            else
                            {
                                int index = group.Teams.FindIndex(gt => gt.Id == team.TeamId);
                                vm.Place = index + 1;
                                vm.Ratio = rTeam.TennisInfo.PlayersSetsLost + "-" + rTeam.TennisInfo.PlayersSetsWon;
                                if (rTeam.TennisInfo.Matches == 0)
                                {
                                    vm.SuccsessLevel = 0;
                                }
                                else
                                {
                                    double wins = rTeam.TennisInfo.Wins;
                                    double games = rTeam.TennisInfo.Matches;
                                    double ratio = (wins / games) * 100;
                                    vm.SuccsessLevel = Convert.ToInt32(ratio);
                                }
                            }
                        }
                        break;
                    }
                }
            }
            
            return vm;
        }

        internal static List<TeamJobsViewModel> GetTeamJobsByTeamId(int teamId, int currentUserId, int? seasonId = null)
        {
            using (DataEntities db = new DataEntities())
            {
                var jobs = new List<TeamJobsViewModel>();
                if(seasonId != null)
                {
                    jobs = db.Users.SelectMany(u => u.UsersJobs, (u, j) => new {u, j})
                        .Where(t =>
                            t.j.TeamId == teamId && t.u.IsArchive == false && t.u.IsActive &&
                            t.j.SeasonId == seasonId)
                        .ToList()
                        .Select(t => new TeamJobsViewModel
                        {
                            Id = t.j.Id,
                            JobName = t.j.Job.JobName,
                            UserId = t.u.UserId,
                            FullName = t.u.FullName?.Trim()
                            //BirthDay = u.BirthDay
                        })
                        .ToList();
                }
                else
                {
                    jobs = db.Users.SelectMany(u => u.UsersJobs, (u, j) => new {u, j})
                        .Where(t => t.j.TeamId == teamId && t.u.IsArchive == false && t.u.IsActive)
                        .ToList()
                        .Select(t => new TeamJobsViewModel
                        {
                            Id = t.j.Id,
                            JobName = t.j.Job.JobName,
                            UserId = t.u.UserId,
                            FullName = t.u.FullName?.Trim()
                            //BirthDay = u.BirthDay
                        })
                        .ToList();

                }

                if (currentUserId != 0)
                {
                    foreach (var job in jobs)
                    {
                        job.FriendshipStatus = FriendsService.AreFriends(job.UserId, currentUserId);
                    }
                }
                return jobs;
            }
        }

        public static List<RankTeam> GetRankedTeams(int leagueId, int teamId, int? seasonId)
        {
            var resList = new List<RankTeam>();

            RankLeague rLeague = CacheService.CreateLeagueRankTable(leagueId, seasonId);

            if (rLeague != null)
            {
                var stage = rLeague.Stages.OrderByDescending(t => t.Number).FirstOrDefault();
                if (stage == null)
                {
                    return null;
                }

                var group = stage.Groups.FirstOrDefault(gr => gr.Teams.Any(t => t.Id == teamId));
                if (group == null)
                {
                    return null;
                }

                var teams = group.Teams.OrderBy(t => t.Position).ToList();
                for (int i = 0; i < teams.Count; i++)
                {
                    if (teams[i].Id == teamId)
                    {
                        if (i > 0)
                            resList.Add(teams[i - 1]);

                        resList.Add(teams[i]);

                        if (i < teams.Count - 1)
                            resList.Add(teams[i + 1]);
                    }
                }
            }

            return resList;
        }
        
        internal static List<UserBaseViewModel> GetTeamFans(int teamId, int leagueId, int currentUserId)
        {
            using (DataEntities db = new DataEntities())
            {
                return db.TeamsFans.Select(tf => new {tf, u = tf.User})
                    .SelectMany(
                        t1 => t1.u.Friends.Where(t => t.IsConfirmed && t.UserId == currentUserId).DefaultIfEmpty(),
                        (t1, us) => new {t1, us})
                    .SelectMany(
                        t1 => t1.t1.u.UsersFriends.Where(t => t.IsConfirmed && t.UserId == currentUserId)
                            .DefaultIfEmpty(), (t1, usf) => new {t1, usf})
                    .Where(t1 => t1.t1.t1.tf.TeamId == teamId)
                    .ToList()
                    .Select(t1 => new UserBaseViewModel
                    {
                        Id = t1.t1.t1.tf.UserId,
                        UserName = t1.t1.t1.u.UserName,
                        FullName = t1.t1.t1.u.UserName,
                        UserRole = t1.t1.t1.u.UsersType.TypeRole,
                        Image = t1.t1.t1.u.Image,
                        CanRcvMsg = true,
                        FriendshipStatus = (t1.t1.us == null && t1.usf == null)
                            ? FriendshipStatus.No
                            : t1.t1.us != null && t1.t1.us.IsConfirmed || t1.usf != null && t1.usf.IsConfirmed
                                ? FriendshipStatus.Yes
                                : FriendshipStatus.Pending
                    })
                    .Distinct()
                    .ToList();
            }
        }

        //internal static List<TeamFanViewModel> GetTeamFans(int leagueId, int teamId)
        //{
        //    using (DataEntities db = new DataEntities())
        //    {
        //        var teamFans = (from teamFan in db.TeamsFans
        //                        join user in db.Users on teamFan.UserId equals user.UserId
        //                        where teamFan.TeamId == teamId && user.IsActive// && teamFan.LeageId == leagueId
        //                        select new TeamFanViewModel
        //                        {
        //                            Id = teamFan.UserId,
        //                            UserName = user.UserName,
        //                            FullName = user.FullName,
        //                            Image = user.Image
        //                        }).ToList();

        //        teamFans = teamFans.GroupBy(x => x.Id).Select(g => g.First()).ToList();

        //        return teamFans;
        //    }
        //}

        //internal static List<UserBaseViewModel> GetTeamFans(Team team, int leagueId, int currentUserId)
        //{

        //    var fans = team.TeamsFans
        //        .Where(tf => tf.LeageId == leagueId && tf.User.IsActive == true && tf.User.IsArchive == false)
        //        .Select(tf => new UserBaseViewModel
        //            {
        //                Id = tf.UserId,
        //                UserName = tf.User.UserName,
        //                Image = tf.User.Image,
        //            }).ToList();
        //    FriendsService.AreFansFriends(fans, currentUserId);
        //    return fans;
        //}

        internal static List<FanTeamsViewModel> GetFanTeams(User user)
        {

            return user.TeamsFans
                .Where(tf => tf.Team.IsArchive == false)
                .Select(tf =>
                new FanTeamsViewModel
                {
                    TeamId = tf.TeamId,
                    Title = tf.Team.Title,
                    LeagueId = tf.LeageId
                }).ToList();
        }

        internal static List<TeamInfoViewModel> GetFanTeamsWithStatistics(User user, int? seasonId)
        {
            /** 
            Func<TeamsFan, bool> predicate;
            if (seasonId.HasValue)
            {
                predicate = tf => tf.Team.IsArchive == false &&
                                  tf.Team.LeagueTeams.Where(x => x.SeasonId == seasonId).
                                  Any(l => l.LeagueId == tf.LeageId);
            }
            else
            {
                predicate = tf => tf.Team.IsArchive == false &&
                                  tf.Team.LeagueTeams.
                                  Any(l => l.LeagueId == tf.LeageId); ;
            }

            return user.TeamsFans
                       .Where(predicate)
                       .Select(tf => GetTeamInfo(tf.Team, tf.LeageId, seasonId)).ToList();
            */
            List<TeamInfoViewModel> tvm = new List<TeamInfoViewModel>();
            foreach (TeamsFan tf in user.TeamsFans)
            {
                int? leagueId = tf.Team.LeagueTeams.Where(lt => lt.TeamId == tf.TeamId && lt.SeasonId == seasonId)?.FirstOrDefault()?.LeagueId;
                if(leagueId != null)
                {
                    tvm.Add(GetTeamInfo(tf.Team, (int)leagueId, seasonId));
                }
            }
            return tvm;
        }

        public Team GetTeamById(int teamId)
        {
            return _db.Teams.Include(x=>x.LeagueTeams)
                    .Include(x=>x.HomeTeamGamesCycles)
                    .Include(x=>x.GuestTeamGamesCycles)
                    .Include(x=>x.HomeTeamGamesCycles.Select(f=>f.Stage))
                    .Include(x=>x.GuestTeamGamesCycles.Select(f=>f.Stage))
                    .Include(x=>x.HomeTeamGamesCycles.Select(f=>f.GameSets))
                    .Include(x=>x.GuestTeamGamesCycles.Select(f=>f.GameSets))
                    .FirstOrDefault(x=>x.TeamId == teamId);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}