using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AppModel;
using DataService;
using WebApi.Models;
using System;

namespace WebApi.Services
{
    public static class ClubService
    {
        private static readonly JobsRepo JRepo = new JobsRepo();

        internal static ClubInfoViewModel GetClub(int clubId, int? seasonId, bool isTennis)
        {
            using (var db = new DataEntities())
            {
                var club = db.Clubs.Include(x => x.ClubTeams)
                                   .Include(x => x.ClubTeams.Select(t => t.Team.TeamsDetails))
                                   .Include(x => x.ClubTeams.Select(f => f.Team.TeamsPlayers)).FirstOrDefault(x => x.ClubId == clubId);
                if (club == null) return new ClubInfoViewModel();

                var result = new ClubInfoViewModel
                {
                    Main = new Main
                    {
                        Players = club.TeamsPlayers.Where(x => x.SeasonId == seasonId && x.IsApprovedByManager == true).
                        Select(x => x.UserId).Distinct().Count(),
                        Fans = club.ClubTeams.Sum(x => x.Team.TeamsFans.Count),
                        Officials = JRepo.CountOfficialsInClub(clubId, seasonId)
                    },
                    Info = new Info
                    {
                        ClubName = club.Name,
                        Address = club.Address,
                        Description = club.Description,
                        Email = club.Email,
                        TermsCondition = club.TermsCondition,
                        Phone = club.ContactPhone,
                        AboutClub = club.IndexAbout,
                        Logo = club.Logo,
                        Image = club.PrimaryImage,
                        Index = club.IndexImage
                    },
                    Officials = JRepo.GetClubOfficials(clubId, seasonId).Select(x => new Officials
                    {
                        JobName = x.JobName,
                        UserName = x.FullName
                    }).ToArray(),
                    Tournaments = db.Leagues.Include(x => x.Age).Include(x => x.Gender).Where(x => x.ClubId == clubId && x.IsArchive == false).Select(x => new Tournaments
                    {
                        Name = x.Name,
                        Ages = x.Age.Title,
                        Gender = x.Gender.Title
                    }).ToArray()
                };

                if (seasonId.HasValue)
                {
                    result.Teams = (from clubTeam in club.ClubTeams
                                    where clubTeam.SeasonId == seasonId
                                    let teamDetails = clubTeam.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                    select new Teams()
                                    {
                                        Id = clubTeam.TeamId,
                                        Team = teamDetails != null ? teamDetails.TeamName : clubTeam.Team.Title,
                                        Logo = teamDetails != null ? teamDetails.Team.Logo : clubTeam.Team.Logo,
                                        ParentId = clubTeam.ClubId,
										Players = clubTeam.Team.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && tp.IsApprovedByManager == true).Select(tp => tp.UserId).Distinct().Count(),
                                        Fans = clubTeam.Team.TeamsFans.Count,
                                        IsSchoolTeam = false,
                                        IsTrainingTeam = clubTeam.IsTrainingTeam // add Cheng Li.
                                    }).ToArray();
                }
                else
                {
                    result.Teams = club.ClubTeams.Select(x => new Teams
                    {
                        Id = x.TeamId,
                        Team = x.Team.Title,
                        Logo = x.Team.Logo,
                        ParentId = club.ClubId,
                        IsSchoolTeam = false,
                        IsTrainingTeam = x.IsTrainingTeam // add Cheng Li.
                    }).ToArray();
                }

                // Cheng Li. set LeagueId
                for(var i = 0; i < result.Teams.Length; i ++)
                {
                    if (result.Teams[i].IsTrainingTeam)
                        continue;

                    var team = result.Teams[i];
                    /**
                    var gamesDb = db.GamesCycles.Where(g => g.HomeTeamId == team.Id).FirstOrDefault();
                    if (gamesDb != null && gamesDb.Stage != null && gamesDb.Stage.League != null && gamesDb.Stage.League.SeasonId == seasonId.Value)
                    {
                        result.Teams[i].ParentId = gamesDb.Stage.LeagueId;
                        result.Teams[i].IsLeagueTeam = true;
                    }
                    */
                    if (isTennis)
                    {
                        var teamRegistrations = db.TeamRegistrations.Where(t => t.ClubId == clubId && t.TeamId == team.Id && t.SeasonId == seasonId.Value).FirstOrDefault();
                        if (teamRegistrations != null)
                        {
                            result.Teams[i].ParentId = teamRegistrations.LeagueId;
                            result.Teams[i].IsLeagueTeam = true;
                        }
                        else
                        {
                            result.Teams[i].IsLeagueTeam = false;
                        }
                    }
                    else
                    {
                        var leagueTeams = db.LeagueTeams.Where(lt => lt.TeamId == team.Id && lt.SeasonId == seasonId.Value).FirstOrDefault();
                        if (leagueTeams != null)
                        {
                            result.Teams[i].ParentId = leagueTeams.LeagueId;
                            result.Teams[i].IsLeagueTeam = true;
                            result.Teams[i].Team += " - " + leagueTeams.Leagues.Name;
                        }
                        else
                        {
                            result.Teams[i].IsLeagueTeam = false;
                        }
                    }
                }

                foreach (var school in club.Schools)
                {
                    var teams = school.SchoolTeams.Select(x => new Teams
                    {
                        Id = x.TeamId,
                        Team = x.Team.Title,
                        Logo = x.Team.Logo,
                        IsSchoolTeam = true,
                        IsTrainingTeam = false
                    }).ToArray();
                    for(var i = 0; i < teams.Length; i++)
                    {
                        var schoolTeam = teams[i];
                        var league = db.LeagueTeams.Where(l => l.TeamId == schoolTeam.Id).FirstOrDefault();
                        if(league != null)
                        {
                            teams[i].ParentId = league.LeagueId;
                        } else
                        {
                            teams[i].ParentId = club.ClubId;
                            teams[i].IsSchoolTeam = false;
                        }
                        for(var k = 0; k < result.Teams.Length; k++)
                        {
                            if(result.Teams[k].Id == teams[i].Id)
                            {
                                result.Teams = result.Teams.RemoveAt(k);
                                continue;
                            }
                        }
                    }
                    result.Teams = result.Teams.Union(teams).ToArray();
                }

                return result;
            }

        }

        internal static RosterInfoViewModel GetPlayersForClub(int clubId, int currentUserId, int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var vm = new RosterInfoViewModel
                {
                    players = new List<CompactPlayerViewModel>(),
                    teamJobs = new List<TeamJobsViewModel>()
                };

                var club = db.Clubs.Include(x => x.ClubTeams)
                    .Include(x => x.ClubTeams.Select(t => t.Team.TeamsDetails))
                    .Include(x => x.ClubTeams.Select(f => f.Team.TeamsPlayers))
                    .FirstOrDefault(x => x.ClubId == clubId);
                if (club != null)
                {
                    // add officials for club
                    if (club.UsersJobs != null && club.UsersJobs.Count > 0)
                    {
                        var jobs = club.UsersJobs
                            .ToList()
                            .Select(u => new TeamJobsViewModel
                            {
                                Id = u.Id,
                                JobName = u.Job.JobName,
                                UserId = u.UserId,
                                FullName = u.User.FullName.Trim(),
                                FriendshipStatus = FriendsService.AreFriends(u.UserId, currentUserId),
                            })
                            .ToList();
                        vm.teamJobs.AddRange(jobs);
                    }
                    if (club.UsersJobs1 != null && club.UsersJobs1.Count > 0)
                    {
                        var jobs = club.UsersJobs1
                            .ToList()
                            .Select(u => new TeamJobsViewModel
                            {
                                Id = u.Id,
                                JobName = u.Job.JobName,
                                UserId = u.UserId,
                                FullName = u.User.FullName.Trim(),
                                FriendshipStatus = FriendsService.AreFriends(u.UserId, currentUserId),
                            })
                            .ToList();
                        vm.teamJobs.AddRange(jobs);
                    }

                    foreach (var team in club.ClubTeams)
                    {
                        vm.players.AddRange(seasonId != null ? PlayerService.GetActivePlayersByClubTeam(team.TeamId, seasonId.Value, currentUserId, clubId, false) :
                                                                  new List<CompactPlayerViewModel>());
                        // add officials for team
                        // Cheng Li. if page of club, only show club officials.
                        // vm.teamJobs.AddRange(TeamsService.GetTeamJobsByTeamId(team.TeamId, currentUserId, seasonId));
                    }
                }
                vm.players = vm.players.GroupBy(x => x.Id).Select(g => g.First()).ToList();
                vm.players = vm.players.OrderBy(x => x.FullName).ToList();
                return vm;
            }
        }
        internal static List<UserBaseViewModel> GetFansForClub(int clubId, int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var fans = new List<UserBaseViewModel>();

                var club = db.Clubs.Include(x => x.ClubTeams)
                                   .Include(x => x.ClubTeams.Select(t => t.Team.TeamsDetails))
                                   .Include(x => x.ClubTeams.Select(f => f.Team.TeamsPlayers)).FirstOrDefault(x => x.ClubId == clubId);
                if (club == null) return fans;

                foreach (var team in club.ClubTeams)
                {
                    foreach (var fan in team.Team.TeamsFans)
                    {

                        var fanModel = new UserBaseViewModel
                        {
                            UserName = fan.User.UserName,
                            Id = fan.User.UserId,
                            Image = fan.User.Image == null && fan.User.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(fan.User, seasonId).Image : fan.User.Image,
                            UserRole = fan.User.UsersType.TypeRole,
                            CanRcvMsg = true
                        };
                        fans.Add(fanModel);
                    }
                }
                fans = fans.GroupBy(i => i.Id).Select(g => g.First()).ToList();
                return fans;
            }
        }
        internal static List<UserBaseViewModel> GetClubsOfCurUser(User user, int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var fans = new List<UserBaseViewModel>();

                var club = db.Clubs.Include(x => x.ClubTeams)
                                   .Include(x => x.ClubTeams.Select(t => t.Team.TeamsDetails))
                                   .Include(x => x.ClubTeams.Select(f => f.Team.TeamsPlayers)).FirstOrDefault(x => x.SeasonId == seasonId);
                if (club == null) return fans;

                foreach (var team in club.ClubTeams)
                {
                    foreach (var fan in team.Team.TeamsFans)
                    {

                        var fanModel = new UserBaseViewModel
                        {
                            UserName = fan.User.UserName,
                            Id = fan.User.UserId,
                            Image = fan.User.Image == null && fan.User.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(fan.User, seasonId).Image : fan.User.Image,
                            UserRole = fan.User.UsersType.TypeRole,
                            CanRcvMsg = true
                        };
                        fans.Add(fanModel);
                    }
                }
                fans = fans.GroupBy(i => i.Id).Select(g => g.First()).ToList();
                return fans;
            }
        }
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            var dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

    }
}