using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using WebApi.Services;
using DataService;
using PushServiceLib;
using WebApi.Models;
using AppModel;
using System.Configuration;
using System.IO;
using WebApi.Photo;
using System.Drawing;
using System.Data.Entity;
using System.Drawing.Imaging;
using Resources;

namespace WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Notifications")]
    public class NotificationsController : BaseLogLigApiController
    {
        NotesMessagesRepo nRepo = new NotesMessagesRepo();
        UsersRepo usersRepo = new UsersRepo();
        UnionsRepo unionRepo = new UnionsRepo();
        LeagueRepo leagueRepo = new LeagueRepo();
        ClubsRepo clubRepo = new ClubsRepo();
        TeamsRepo teamRepo = new TeamsRepo();
        JobsRepo jobRepo = new JobsRepo();
        TeamTrainingsRepo trainingRepo = new TeamTrainingsRepo();

        [Route("list")]
        public IHttpActionResult GetList()
        {
            var resList = nRepo.GetByUser(CurrUserId)
                .Where(n => (n.NotesMessage.TypeId != MessageTypeEnum.ChatMessage) )
                .Select(t => new NotificationsViewModel
                    {
                        MsgId = t.MsgId,
                        IsRead = t.IsRead,
                        Message = t.NotesMessage.Message,
                        SendDate = t.NotesMessage.SendDate
                    });
            return Ok(resList);
        }

        [Route("chats/users")]
        public IHttpActionResult GetChatUsers(int? unionId = null)
        {
            var usr = CurrentUser;//usersRepo.GetById(uid);//
            if (usr == null)
            {
                return NotFound();
            }
            
            var seasonsRepo = new SeasonsRepo();
            var seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId((int)unionId) : (int?)null;
            //if(unionId == 15)
            //{
            //    unionId = db.UsersJobs.Where(x => x.UserId == usr.UserId).Select(x => x.UnionId).FirstOrDefault();
            //    if(unionId == null)
            //    {
            //        var club = CurrentUser.UsersJobs?.Where(x => x.ClubId != null).FirstOrDefault()?.Club;
            //        if(club != null)
            //        {
            //            seasonId = seasonsRepo.GetLastSeasonByCurrentClub(club).Id;
            //        }
            //        else
            //        {
            //            seasonId = (from tps in db.TeamsPlayers
            //                        from users in db.Users
            //                        where tps.UserId == users.UserId
            //                        select new
            //                        {
            //                            sid = tps.Club.SeasonId
            //                        }).OrderByDescending(x => x.sid).FirstOrDefault()?.sid;
            //            if(seasonId == null)
            //            {
            //                seasonId = (from tps in db.TeamsFans
            //                            from users in db.Users
            //                            where tps.UserId == users.UserId
            //                            select new
            //                            {
            //                                sid = tps.League.Club.SeasonId
            //                            }).OrderByDescending(x => x.sid).FirstOrDefault()?.sid;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId((int)unionId) : (int?)null;
            //    }
                
            //}
            var vm = new ChatUserViewModel
            {
                Friends = FriendsService.GetAllConfirmedFriendsAsUsers(usr)
                    .Select(u =>
                        new ChatUserModel
                        {
                            UserId = u.UserId,
                            UserName = u.UserName ?? u.FullName,
                            // Cheng Li. Add : UserRole field
                            UserRole = u.UsersType.TypeRole
                        })
                    .Where(u => u.UserId != this.CurrUserId)
                    .ToList()
            };
            //Friends

            if (User.IsInRole(AppRole.Fans))
                vm.JobRole = AppRole.Fans;
            else
            {
                vm.JobRole = usersRepo.GetTopLevelJob(usr.UserId);
                if(vm.JobRole == null)
                {
                    vm.JobRole = AppRole.Players;
                }
            }

            if (!User.IsInRole(AppRole.Fans)) {
                switch (usersRepo.GetTopLevelJob(usr.UserId)) {
                    case JobRole.UnionManager:
                        var union = unionRepo.GetById((int)db.UsersJobs.FirstOrDefault(j => j.UserId == usr.UserId && j.UnionId != null && j.SeasonId == seasonId)?.UnionId);
                        vm.GameName = union == null?"":union.Name;
                        this.GetUnionManager(vm, union, usr.UserId, seasonId);
                        break;
                    case JobRole.LeagueManager:
                        var league = leagueRepo.GetById((int)jobRepo.GetLeagueIdByLeagueManagerId(usr.UserId));
                        vm.GameName = league == null ? "" : league.Name;
                        this.GetLeagueManager(vm, league, seasonId);
                        break;
                    case JobRole.ClubManager:
                        vm.Teams = new List<ChatUserModel>();
                        for( var j = 0; j< usr.UsersJobs.Count(); j++)
                        {
                            var club1 = usr.UsersJobs.ElementAt(j).Club;
                            seasonId = seasonsRepo.GetLastSeasonIdByCurrentClubId(club1.ClubId);
                            var departmentlist = db.Clubs.Where(x => x.ParentClubId == club1.ClubId).ToList();
                            departmentlist.Add(club1);
                            for(var k = 0; k< departmentlist.Count(); k++)
                            {
                                var club = departmentlist.ElementAt(k);
                                if (club == null || club.IsArchive == true)
                                {
                                    continue;
                                }
                                vm.GameName = club == null ? "" : club.Name;
                                this.GetClubManager(vm, club, seasonId);
                                var Teams = club.ClubTeams.Where(x => x.IsBlocked == false && x.Team.IsArchive == false && x.SeasonId == seasonId
                                && x.Club.IsArchive == false).Select(x => new ChatUserModel()
                                {
                                    UserId = x.TeamId,
                                    UserName = x.Team.TeamsDetails?.Where(t => t.SeasonId == seasonId)?.FirstOrDefault()?.TeamName ?? x.Team.Title
                                }).ToList().Concat(db.SchoolTeams.Where(x => x.School.ClubId == club.ClubId && x.School.SeasonId == seasonId && x.Team.IsArchive == false).Select(x => new ChatUserModel()
                                {
                                    UserId = x.TeamId,
                                    UserName = x.Team.Title
                                })).ToList();
                                if (Teams.Count() == 0)
                                {
                                    Teams = club.Leagues?.Where(g => g.IsArchive == false).OrderByDescending(g => g.SeasonId).FirstOrDefault()?.LeagueTeams.Where(x => x.Teams.IsArchive == false
                                    && x.SeasonId == seasonId).Select(x => new ChatUserModel()
                                    {
                                        UserId = x.TeamId,
                                        UserName = x.Teams.TeamsDetails?.Where(t => t.SeasonId == seasonId).FirstOrDefault()?.TeamName ?? x.Teams.Title
                                    }).Distinct().ToList();
                                }

                                if (Teams == null) continue;
                                for (var i = 0; i < Teams?.Count; i++)
                                {
                                    var teamId = Teams[i].UserId;
                                    var leagueTeams = db.LeagueTeams.Where(lt => lt.TeamId == teamId && lt.SeasonId == seasonId && lt.Teams.IsArchive == false).FirstOrDefault();
                                    if (leagueTeams != null)
                                    {
                                        Teams[i].UserName += " - " + leagueTeams.Leagues.Name;
                                    }
                                }
                                vm.Teams.AddRange(Teams.GroupBy(x => x.UserId).Select(x => x.FirstOrDefault()));
                            }
                        }
                        break;
                    case JobRole.DepartmentManager:
                        vm.Teams = new List<ChatUserModel>();
                        for (var j = 0; j < usr.UsersJobs.Count(); j++)
                        {
                            var club1 = usr.UsersJobs.ElementAt(j).Club;
                            var departmentlist = db.Clubs.Where(x => x.ParentClubId == club1.ClubId).ToList();
                            departmentlist.Add(club1);
                            for (var k = 0; k < departmentlist.Count(); k++)
                            {
                                var club = departmentlist.ElementAt(k);
                                if (club == null)
                                {
                                    continue;
                                }
                                seasonId = seasonsRepo.GetLastSeasonIdByCurrentClubId(club.ClubId);
                                vm.GameName = club == null ? "" : club.Name;
                                this.GetClubManager(vm, club, seasonId);
                                var Teams = club.ClubTeams.Where(x => x.IsBlocked == false && x.Team.IsArchive == false && x.SeasonId == seasonId
                                && x.Club.IsArchive == false).Select(x => new ChatUserModel()
                                {
                                    UserId = x.TeamId,
                                    UserName = x.Team.TeamsDetails?.Where(t => t.SeasonId == seasonId)?.FirstOrDefault()?.TeamName ?? x.Team.Title
                                }).ToList().Concat(db.SchoolTeams.Where(x => x.School.ClubId == club.ClubId && x.School.SeasonId == seasonId && x.Team.IsArchive == false).Select(x => new ChatUserModel()
                                {
                                    UserId = x.TeamId,
                                    UserName = x.Team.Title
                                })).ToList();
                                if (Teams.Count() == 0)
                                {
                                    Teams = club.Leagues?.Where(g => g.IsArchive == false).OrderByDescending(g => g.SeasonId).FirstOrDefault()?.LeagueTeams.Where(x => x.Teams.IsArchive == false).Select(x => new ChatUserModel()
                                    {
                                        UserId = x.TeamId,
                                        UserName = x.Teams.TeamsDetails?.Where(t => t.SeasonId == seasonId).FirstOrDefault()?.TeamName ?? x.Teams.Title
                                    }).Distinct().ToList();
                                }

                                if (Teams == null) continue;
                                for (var i = 0; i < Teams?.Count; i++)
                                {
                                    var teamId = Teams[i].UserId;
                                    var leagueTeams = db.LeagueTeams.Where(lt => lt.TeamId == teamId && lt.SeasonId == seasonId && lt.Teams.IsArchive == false).FirstOrDefault();
                                    if (leagueTeams != null)
                                    {
                                        Teams[i].UserName += " - " + leagueTeams.Leagues.Name;
                                    }
                                }
                                vm.Teams.AddRange(Teams.GroupBy(x => x.UserId).Select(x => x.FirstOrDefault()));
                            }
                        }
                        break;
                    default:
                        if(usersRepo.GetTopLevelJob(usr.UserId) == JobRole.TeamManager || User.IsInRole(AppRole.Players))
                        {
                            seasonId = seasonsRepo.GetLastSeasonByCurrentUnionIdByUser(CurrUserId);
                            if (usr.UsersJobs != null && usr.UsersJobs.Count > 0)
                            {
                                vm.TeamUsers = new List<ChatUserForTeamViewModel>();
                                var usersjobs = usr.UsersJobs.Where(x => x.IsBlocked == false);
                                for (var i = 0; i< usersjobs.Count(); i++)
                                {
                                    if(usersjobs.ElementAt(i).TeamId != null)
                                    {
                                        var x = usersjobs.ElementAt(i);
                                        var teamuser = new ChatUserForTeamViewModel();
                                        teamuser.TeamId = x.TeamId;
                                        teamuser.TeamName = (x.Team == null ? null : x.Team.TeamsDetails?.OrderByDescending(g => g.SeasonId)
                                        .FirstOrDefault()?.TeamName ?? x.Team.Title);
                                        teamuser.IsSelectAll = false;
                                        teamuser.TeamOfficials = jobRepo.GetTeamUsersJobs(x.TeamId ?? -1, x.SeasonId).Select(to => new ChatUserModel
                                        {
                                            UserId = to.UserId,
                                            UserName = to.FullName
                                        }).ToList();
                                        teamuser.Players = trainingRepo.GetPlayersByTeamId(x.TeamId ?? -1, x.SeasonId).Select(py => new ChatUserModel
                                        {
                                            UserId = py.UserId,
                                            UserName = py.User.FullName
                                        }).ToList();
                                        vm.TeamUsers.Add(teamuser);
                                    }
                                    if(usersjobs.ElementAt(i).ClubId != null)
                                    {
                                        for( var j = 0; j< usersjobs.ElementAt(i).Club.ClubTeams.Count(); j++)
                                        {
                                            var clubi = usersjobs.ElementAt(i).Club;
                                            var clubteam = usersjobs.ElementAt(i).Club.ClubTeams.ElementAt(j);
                                            seasonId = seasonsRepo.GetLastSeasonIdByCurrentClubId(clubi.ClubId);
                                            if(clubteam.SeasonId != seasonId)
                                            {
                                                continue;
                                            }
                                            var x = clubteam;
                                            var teamuser = new ChatUserForTeamViewModel();
                                            teamuser.TeamId = x.TeamId;
                                            teamuser.TeamName = (x.Team == null ? null : x.Team.TeamsDetails?.OrderByDescending(g => g.SeasonId)
                                            .FirstOrDefault()?.TeamName ?? x.Team.Title);
                                            teamuser.IsSelectAll = false;
                                            teamuser.TeamOfficials = jobRepo.GetTeamUsersJobs(x.TeamId, x.SeasonId).Select(to => new ChatUserModel
                                            {
                                                UserId = to.UserId,
                                                UserName = to.FullName
                                            }).ToList();
                                            teamuser.Players = trainingRepo.GetPlayersByTeamId(x.TeamId, x.SeasonId).Select(py => new ChatUserModel
                                            {
                                                UserId = py.UserId,
                                                UserName = py.User?.FullName
                                            }).ToList();
                                            vm.TeamUsers.Add(teamuser);
                                        }
                                        for (var k = 0; k < usersjobs.ElementAt(i).Club.Schools.Count(); k++)
                                        {
                                            var clubi = usersjobs.ElementAt(i).Club;
                                            seasonId = seasonsRepo.GetLastSeasonIdByCurrentClubId(clubi.ClubId);
                                            var school = usersjobs.ElementAt(i).Club.Schools.Where(g => g.SeasonId == seasonId);
                                            for (var p = 0; p < school.Count(); p++) {
                                                for(var y = 0; y< school.ElementAt(p).SchoolTeams.Count; y++)
                                                {
                                                    var clubteam = school.ElementAt(p).SchoolTeams.ElementAt(y);
                                                    var x = clubteam;
                                                    var teamuser = new ChatUserForTeamViewModel();
                                                    teamuser.TeamId = x.TeamId;
                                                    teamuser.TeamName = (x.Team == null ? null : x.Team.TeamsDetails?.OrderByDescending(g => g.SeasonId)
                                                    .FirstOrDefault()?.TeamName ?? x.Team.Title);
                                                    teamuser.IsSelectAll = false;
                                                    teamuser.TeamOfficials = jobRepo.GetTeamUsersJobs(x.TeamId, seasonId).Select(to => new ChatUserModel
                                                    {
                                                        UserId = to.UserId,
                                                        UserName = to.FullName
                                                    }).ToList();
                                                    teamuser.Players = trainingRepo.GetPlayersByTeamId(x.TeamId, seasonId).Select(py => new ChatUserModel
                                                    {
                                                        UserId = py.UserId,
                                                        UserName = py.User?.FullName
                                                    }).ToList();
                                                    vm.TeamUsers.Add(teamuser);
                                                }
                                            }
                                        }
                                    }
                                }
                                vm.TeamUsers = vm.TeamUsers.GroupBy(i => i.TeamId).Select(g => g.First()).ToList();

                                foreach (var TeamUser in vm.TeamUsers)
                                {
                                    TeamUser.Users = new List<ChatUserModel>();
                                    if (TeamUser.TeamOfficials.Count() > 0)
                                    {
                                        TeamUser.Users.AddRange(TeamUser.TeamOfficials);
                                    }
                                    TeamUser.Users.AddRange(TeamUser.Players);
                                    TeamUser.Users = TeamUser.Users.Where(u => u.UserId != this.CurrUserId).GroupBy(tu => tu.UserId).Select(g => g.First()).ToList();
                                    TeamUser.TeamOfficials.Clear();
                                    TeamUser.Players.Clear();
                                }
                            }
                            else if(User.IsInRole(AppRole.Players))
                            {
                                vm.TeamUsers = usr.TeamsPlayers.ToList()
                                    .Select(x => new ChatUserForTeamViewModel()
                                {
                                    TeamId = x.TeamId,
                                    TeamName = (x.Team == null ? null : x.Team.TeamsDetails?.OrderByDescending(g=>g.SeasonId).
                                    FirstOrDefault()?.TeamName??x.Team.Title),
                                    IsSelectAll = false,
                                    TeamOfficials = jobRepo.GetTeamUsersJobs((int)x.TeamId, x.Team?.TeamsDetails?.OrderByDescending(g=>g.SeasonId)
                                    .FirstOrDefault()?.SeasonId).Select(to => new ChatUserModel
                                    {
                                        UserId = to.UserId,
                                        UserName = to.FullName
                                    }).GroupBy(t=>t.UserId).Select(g=>g.FirstOrDefault()).ToList(),
                                    Players = trainingRepo.GetPlayersByTeamId((int)x.TeamId, x.Team?.TeamsDetails?.OrderByDescending(g => g.SeasonId)
                                    .FirstOrDefault()?.SeasonId).Select(py => new ChatUserModel
                                    {
                                        UserId = py.UserId,
                                        UserName = py.User.FullName
                                    }).GroupBy(t=>t.UserId).Select(g=>g.FirstOrDefault()).ToList()
                                }).ToList();

                                vm.TeamUsers = vm.TeamUsers.GroupBy(i => i.TeamId).Select(g => g.First()).ToList();


                                foreach (var TeamUser in vm.TeamUsers)
                                {
                                    TeamUser.Users = new List<ChatUserModel>();
                                    TeamUser.Users.AddRange(TeamUser.TeamOfficials);
                                    TeamUser.Users.AddRange(TeamUser.Players);
                                    TeamUser.Users = TeamUser.Users.Where(u => u.UserId != this.CurrUserId).GroupBy(tu => tu.UserId).Select(g => g.First()).ToList();
                                    TeamUser.TeamOfficials.Clear();
                                    TeamUser.Players.Clear();
                                }
                            }
                        }
                        break;
                }
            }
            //--
            // do not display on client screen, but can send player.
            // in this case, sendTeam or sendAllusers
            vm.Teams = vm.Teams?.Where(ttt=>ttt.UserId!= CurrUserId).
                GroupBy(x => x.UserId).Select(t => t.FirstOrDefault()).ToList();
            vm.ClubOfficials = vm.ClubOfficials?.Where(ttt => ttt.UserId != CurrUserId).GroupBy(x => x.UserId).Select(t => t.FirstOrDefault()).ToList();
            vm.TeamOfficials = vm.TeamOfficials?.Where(ttt => ttt.UserId != CurrUserId).GroupBy(x => x.UserId).Select(t => t.FirstOrDefault()).ToList();
            vm.DepartmentOfficials = vm.DepartmentOfficials?.Where(ttt => ttt.UserId != CurrUserId).GroupBy(x => x.UserId).Select(t => t.FirstOrDefault()).ToList();
            vm.Players = vm.Players?.Where(ttt => ttt.UserId != CurrUserId).GroupBy(x => x.UserId).Select(t => t.FirstOrDefault()).ToList();
            vm.TeamUsers = vm.TeamUsers?.GroupBy(x => x.TeamId).Select(t => t.FirstOrDefault()).ToList();
            vm.LeagueOfficials = vm.LeagueOfficials?.Where(ttt => ttt.UserId != CurrUserId).GroupBy(x => x.UserId).Select(t => t.FirstOrDefault()).ToList(); 
            vm.UnionOfficials = vm.UnionOfficials?.Where(ttt => ttt.UserId != CurrUserId).GroupBy(x => x.UserId).Select(t => t.FirstOrDefault()).ToList();

            if (vm.Players != null)
                vm.Players.Clear();
            return Ok(vm);
        }

        /** Cheng Li : Add : for get receives List*/
        [Route("chats/receives/{msgId}")]
        public IHttpActionResult GetChatMessageReceiveList(int msgId)
        {
            var uId = CurrUserId;
            var Receives = db.NotesRecipients.Where(n => (n.MsgId == msgId && n.UserId != uId)).Select(nr => new ReceiveModel
            {
                receiveId = nr.UserId,
                receiveName = db.Users.Where(u => u.UserId == nr.UserId).Select(u => u.UserName).FirstOrDefault()
            }).ToList();

            return Ok(Receives);
        }
        [Route("chats/list")]
        public IHttpActionResult GetChatMessageList()
        {
            var uId = CurrUserId;
            var resList = nRepo.GetByUser(uId)
                .Where(n => n.NotesMessage.TypeId == MessageTypeEnum.ChatMessage &&
                            (n.NotesMessage.parent == 0 || n.NotesMessage.parent == null))
                .ToList()
                .Select(t => new ChatViewModel
                {
                    MsgId = t.MsgId,
                    IsRead = t.IsRead,
                    Message = t.NotesMessage.Message,
                    SendDate = t.NotesMessage.SendDate,
                    SenderId = t.NotesMessage.Sender,
                    SenderName = t.NotesMessage.SendUser.UserName ?? t.NotesMessage.SendUser.FullName,
                    SenderImage = t.NotesMessage.SendUser.Image,
                    img = t.NotesMessage.img,
                    video = t.NotesMessage.video,
                    imgUrl = t.NotesMessage.img,
                    videoUrl = t.NotesMessage.video,
                    Childs = t.NotesMessage.Childs.Select(tt => new CommentViewModel
                    {
                        MsgId = tt.MsgId,
                        Message = tt.Message,
                        SendDate = tt.SendDate,
                        SenderId = tt.Sender,
                        SenderName = tt.SendUser.UserName ?? tt.SendUser.FullName,
                        img = tt.img,
                        video = tt.video
                    }).ToList()
                })
                .ToList();

            for (var i = 0; i < resList.Count; i ++)
            {
                var cvm = resList.ElementAt(i);
                cvm.Receives = db.NotesRecipients
                    .Where(n => n.MsgId == cvm.MsgId && n.UserId != uId)
                    .ToList()
                    .Select(nr => new ReceiveModel
                    {
                        receiveId = nr.UserId,
                        receiveName = db.Users.FirstOrDefault(u => u.UserId == nr.UserId)?.FullName,
                        shortName = db.Users.FirstOrDefault(u => u.UserId == nr.UserId)?.UserName
                    })
                    .ToList();

                if(cvm.SenderImage == null)
                {
                    cvm.SenderImage = db.PlayerFiles.Where(x => x.PlayerId == cvm.SenderId && x.FileType == (int)PlayerFileType.PlayerImage).Select(x => x.FileName).FirstOrDefault()?? cvm.SenderImage;
                }
            }

            resList = resList.OrderByDescending(x => x.SendDate).ToList();
            return Ok(resList);
        }

        private void SendAllUsers(NotesMessagesRepo.PostChatMessage chatmsg, bool bFoward = false, int? unionId = null)
        {
            var usr = CurrentUser;//usersRepo.GetById(uid);//
            if (usr == null)
            {
                return;
            }

            var seasonsRepo = new SeasonsRepo();
            var seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId((int)unionId) : (int?)null;

            var vm = new ChatUserViewModel
            {
                Friends = FriendsService.GetAllConfirmedFriendsAsUsers(usr).Select(u =>
                        new ChatUserModel
                        {
                            UserId = u.UserId,
                            UserName = u.UserName ?? u.FullName,
                            // Cheng Li. Add : UserRole field
                            UserRole = u.UsersType.TypeRole
                        })
                    .Where(u => u.UserId != this.CurrUserId)
                    .ToList()
            };
            //Friends

            if (User.IsInRole(AppRole.Fans))
                vm.JobRole = AppRole.Fans;
            else
            {
                vm.JobRole = usersRepo.GetTopLevelJob(usr.UserId);
                if (vm.JobRole == null)
                {
                    vm.JobRole = AppRole.Players;
                }
            }

            if (!User.IsInRole(AppRole.Fans))
            {
                switch (usersRepo.GetTopLevelJob(usr.UserId))
                {
                    case JobRole.UnionManager:
                        var union = unionRepo.GetById((int)db.UsersJobs.FirstOrDefault(j => j.UserId == usr.UserId && j.UnionId != null && j.SeasonId == seasonId)?.UnionId);
                        vm.GameName = union == null ? "" : union.Name;
                        this.GetUnionManager(vm, union, usr.UserId, seasonId);
                        break;
                    case JobRole.LeagueManager:
                        var league = leagueRepo.GetById((int)jobRepo.GetLeagueIdByLeagueManagerId(usr.UserId));
                        vm.GameName = league == null ? "" : league.Name;
                        this.GetLeagueManager(vm, league, seasonId);
                        break;
                    case JobRole.ClubManager:
                        var club = clubRepo.GetById((int)jobRepo.GetClubIdByClubManagerId(usr.UserId));
                        vm.GameName = club == null ? "" : club.Name;
                        this.GetClubManager(vm, club, seasonId);
                        vm.Teams = club.ClubTeams.Select(x => new ChatUserModel()
                        {
                            UserId = x.TeamId,
                            UserName = x.Team.Title
                        }).ToList();
                        break;
                    default:
                        if (usersRepo.GetTopLevelJob(usr.UserId) == JobRole.TeamManager || User.IsInRole(AppRole.Players))
                        {
                            if (usr.UsersJobs != null && usr.UsersJobs.Count > 0)
                            {
                                vm.TeamUsers = usr.UsersJobs.Where(x => x.SeasonId == seasonId).Select(x => new ChatUserForTeamViewModel()
                                {
                                    TeamId = x.TeamId,
                                    TeamName = (x.Team == null ? null : x.Team.Title),
                                    IsSelectAll = false,
                                    TeamOfficials = jobRepo.GetTeamUsersJobs((int)x.TeamId, seasonId).Select(to => new ChatUserModel
                                    {
                                        UserId = to.UserId,
                                        UserName = to.FullName
                                    }).ToList(),
                                    Players = trainingRepo.GetPlayersByTeamId((int)x.TeamId, seasonId).Select(py => new ChatUserModel
                                    {
                                        UserId = py.UserId,
                                        UserName = py.User.FullName
                                    }).ToList()
                                }).ToList();

                                foreach (var TeamUser in vm.TeamUsers)
                                {
                                    TeamUser.Users = new List<ChatUserModel>();
                                    TeamUser.Users.AddRange(TeamUser.TeamOfficials);
                                    TeamUser.Users.AddRange(TeamUser.Players);
                                    TeamUser.Users = TeamUser.Users.GroupBy(tu => tu.UserId).Select(g => g.First()).ToList();
                                    TeamUser.TeamOfficials.Clear();
                                    TeamUser.Players.Clear();
                                }


                            }
                            else if (User.IsInRole(AppRole.Players))
                            {
                                vm.TeamUsers = usr.TeamsPlayers.Where(x => x.SeasonId == seasonId).Select(x => new ChatUserForTeamViewModel()
                                {
                                    TeamId = x.TeamId,
                                    TeamName = (x.Team == null ? null : x.Team.Title),
                                    IsSelectAll = false,
                                    TeamOfficials = jobRepo.GetTeamUsersJobs((int)x.TeamId, seasonId).Select(to => new ChatUserModel
                                    {
                                        UserId = to.UserId,
                                        UserName = to.FullName
                                    }).ToList(),
                                    Players = trainingRepo.GetPlayersByTeamId((int)x.TeamId, seasonId).Select(py => new ChatUserModel
                                    {
                                        UserId = py.UserId,
                                        UserName = py.User.FullName
                                    }).ToList()
                                }).ToList();

                                foreach (var TeamUser in vm.TeamUsers)
                                {
                                    TeamUser.Users = new List<ChatUserModel>();
                                    TeamUser.Users.AddRange(TeamUser.TeamOfficials);
                                    TeamUser.Users.AddRange(TeamUser.Players);
                                    TeamUser.Users = TeamUser.Users.GroupBy(tu=> tu.UserId).Select(g => g.First()).ToList();
                                    TeamUser.TeamOfficials.Clear();
                                    TeamUser.Players.Clear();
                                }
                            }
                        }
                        break;
                }
            }
            //--
            int i = 0, k = 0;
            if(vm.UnionOfficials != null)
            {
                for (i = 0; i < vm.UnionOfficials.Count; i++)
                {
                    chatmsg.Users.Add(vm.UnionOfficials.ElementAt(i).UserId);
                }
            }
            if(vm.LeagueOfficials != null)
            {
                for (i = 0; i < vm.LeagueOfficials.Count; i++)
                {
                    chatmsg.Users.Add(vm.LeagueOfficials.ElementAt(i).UserId);
                }
            }
            if(vm.ClubOfficials != null)
            {
                for (i = 0; i < vm.ClubOfficials.Count; i++)
                {
                    chatmsg.Users.Add(vm.ClubOfficials.ElementAt(i).UserId);
                }
            }
            if(vm.TeamOfficials != null)
            {
                for (i = 0; i < vm.TeamOfficials.Count; i++)
                {
                    chatmsg.Users.Add(vm.TeamOfficials.ElementAt(i).UserId);
                }
            }
            if(vm.TeamUsers != null)
            {
                for (i = 0; i < vm.TeamUsers.Count; i++)
                {
                    for (k = 0; k < vm.TeamUsers.ElementAt(i).Users.Count; k++)
                    {
                        chatmsg.Users.Add(vm.TeamUsers.ElementAt(i).Users.ElementAt(k).UserId);
                    }
                    for (k = 0; k < vm.TeamUsers.ElementAt(i).TeamOfficials.Count; k++)
                    {
                        chatmsg.Users.Add(vm.TeamUsers.ElementAt(i).TeamOfficials.ElementAt(k).UserId);
                    }
                    for (k = 0; k < vm.TeamUsers.ElementAt(i).Players.Count; k++)
                    {
                        chatmsg.Users.Add(vm.TeamUsers.ElementAt(i).Players.ElementAt(k).UserId);
                    }
                }
            }
            if(vm.Friends != null)
            {
                for (i = 0; i < vm.Friends.Count; i++)
                {
                    chatmsg.Users.Add(vm.Friends.ElementAt(i).UserId);
                }
            }
            if(vm.Players != null)
            {
                for (i = 0; i < vm.Players.Count; i++)
                {
                    chatmsg.Users.Add(vm.Players.ElementAt(i).UserId);
                }
            }
            if(chatmsg.Users != null && chatmsg.Users.Count > 0)
            {
                chatmsg.Users = chatmsg.Users.Distinct().ToList();
                if (!bFoward)
                    nRepo.SendChatToUsers(chatmsg, CurrUserId);
                else
                    nRepo.ForwarcChatToUsers((int)chatmsg.SenderId, chatmsg.Users); // chatmsg.SenderId is messageid.
            }
        }
        [Route("chats/send")]
        public IHttpActionResult PostChatToUsers(NotesMessagesRepo.PostChatMessage chatmsg, int? unionId = null)
        {
            if (!chatmsg.SendAllFlag)
                nRepo.SendChatToUsers(chatmsg, CurrUserId);
            else
                SendAllUsers(chatmsg, false, unionId);

            return Ok();
        }
        [Route("chats/sendTeam")]
        public IHttpActionResult PostChatToTeamUsers(NotesMessagesRepo.PostChatMessage chatmsg, int? unionId = null)
        {
            var seasonsRepo = new SeasonsRepo();
            var seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId((int)unionId) : (int?)null;


            var userMsg = new NotesMessagesRepo.PostChatMessage();
            userMsg.img = chatmsg.img;
            userMsg.Message = chatmsg.Message;
            userMsg.SenderId = chatmsg.SenderId;
            userMsg.parent = chatmsg.parent;
            userMsg.video = chatmsg.video;

            if (chatmsg.Users.Count > 0)
            {
                userMsg.Users = new List<int>();
                foreach (var teamId in chatmsg.Users.Distinct())
                {
                    userMsg.Users.AddRange(
                        jobRepo.GetTeamUsersJobs(teamId, seasonId).Select(x => x.UserId).ToList()
                    );
                    userMsg.Users.AddRange(
                        trainingRepo.GetPlayersByTeamId(teamId, seasonId).Select(x => x.UserId).ToList()
                    );
                }
                nRepo.SendChatToUsers(userMsg, CurrUserId);
            }
            return Ok();
        }

        [Route("chats/forward")]
        public IHttpActionResult ForwadChatToUsers(int[] value)
        {
            var msgId = value[0];
            var friends = new List<int>();
            for(var i = 1; i < value.Length; i ++)
            {
                friends.Add(value.ElementAt(i));
            }
            nRepo.ForwarcChatToUsers(msgId, friends);
            //nRepo.ForwarcChatToUsers(fwModel.MsgId, fwModel.Friends);
            return Ok();
        }
        [Route("chats/forwardAll")]
        public IHttpActionResult ForwadChatAllToUsers(int[] value, int? unionId = null)
        {
            var fwdMsgId = value[0];
            var chatmsg = new NotesMessagesRepo.PostChatMessage();
            chatmsg.Users = new List<int>();
            chatmsg.SenderId = fwdMsgId;
            SendAllUsers(chatmsg, true, unionId);
            return Ok();
        }

        [Route("chats/forwardTeam")]
        public IHttpActionResult ForwadChatTeamToUsers(int[] value, int? unionId = null)
        {
            var seasonsRepo = new SeasonsRepo();
            var seasonId = unionId != null ? seasonsRepo.GetLastSeasonByCurrentUnionId((int)unionId) : (int?)null;

            var users = new List<int>();
            var msgId = value[0];

            if (value.Length > 1)
            {
                for (var i = 1; i < value.Length; i++)
                {
                    users.AddRange(
                        jobRepo.GetTeamUsersJobs(value.ElementAt(i), seasonId).Select(x => x.UserId).ToList()
                    );
                    users.AddRange(
                        trainingRepo.GetPlayersByTeamId(value.ElementAt(i), seasonId).Select(x => x.UserId).ToList()
                    );
                }

                var friends = new List<int>();
                for (var i = 0; i < users.Count; i++)
                {
                    friends.Add(users.ElementAt(i));
                }
                nRepo.ForwarcChatToUsers(msgId, friends);
            }

            return Ok();
        }

        [Route("chats/delete")]
        public IHttpActionResult PostChatDelete([FromBody]int id)
        {
            nRepo.Delete(id, base.CurrUserId);
            nRepo.Save();

            return Ok();
        }

        [Route("reminder")]
        public IHttpActionResult PostReminder([FromBody]string message)
        {
            //var message = "fsdfdsf";
            var msg = new NotesMessage
            {
                Message = message,
                SendDate = DateTime.Now,
                TypeId = MessageTypeEnum.Root
            };

            db.NotesMessages.Add(msg);
            
            var nr = new NotesRecipient { MsgId = msg.MsgId, UserId = base.CurrUserId, IsPushSent = true };
            db.NotesRecipients.Add(nr);

            db.SaveChanges();

            return Ok();
        }

        [Route("delete")]
        public IHttpActionResult PostDelete([FromBody]int id)
        {
            nRepo.Delete(id, base.CurrUserId);
            nRepo.Save();

            return Ok();
        }

        [Route("deleteAll")]
        public IHttpActionResult PostDeleteAll(int[] msgsArr)
        {
            var nRepo = new NotesMessagesRepo();

            nRepo.DeleteAll(base.CurrUserId, msgsArr);
            nRepo.Save();

            return Ok();
        }

        [Route("readAll")]
        public IHttpActionResult PostReadAll(int[] msgsArr)
        {
            var nRepo = new NotesMessagesRepo();

            nRepo.SetRead(base.CurrUserId, msgsArr);
            nRepo.Save();

            return Ok();
        }

        [Route("saveToken")]
        public async Task<IHttpActionResult> PostSaveToken(TokenItem item)
        {
            var notifyService = new GamesNotificationsService();
            var id = 0;

            await notifyService.SaveUserDeviceToken(CurrUserId, item.Token, id, item.IsIOS, item.Section);

            return Ok();
        }

        [Route("deleteToken")]
        public async Task<IHttpActionResult> PostDeleteToken(TokenItem item)
        {
            var notifyService = new GamesNotificationsService();

            await notifyService.UnregisterDeviceToken(base.CurrUserId, item.Token);

            return Ok();
        }

        /// <summary>
        /// העלאת תמונת פרופיל 
        /// </summary>
        /// <returns></returns>
        [Route("chats/uploadImage")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostUploadChatImage()
        {

            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                try
                {
                    var user = base.CurrentUser;
                    if (user != null)
                    {
                        var appPath = ConfigurationManager.AppSettings["ChatUrl"];
                        var fileName = DateTime.Now.Ticks.ToString() + ".jpeg";
                        var absoluteFolderPath = appPath;
                        var pathToFile = absoluteFolderPath + "\\" + fileName;

                        if (!Directory.Exists(absoluteFolderPath))
                        {
                            Directory.CreateDirectory(absoluteFolderPath);
                        }

                        var provider = new PhotoMultipartFormDataStreamProvider(absoluteFolderPath, fileName);
                        await Request.Content.ReadAsMultipartAsync(provider);

                        //using (var image = Image.FromFile(pathToFile))
                        //{
                        //    var size = image.Height >= image.Width ? image.Width : image.Height;
                        //    var paddingW = (image.Width - size) / 2;
                        //    var padding = (image.Height - size) / 2;
                        //    using (var newImage = Crop(image, new Rectangle(paddingW, padding, image.Width - paddingW * 2, image.Height - padding * 2)))
                        //    {
                        //        newImage.Save(absoluteFolderPath + "\\" + fileName, ImageFormat.Jpeg);
                        //    }
                        //}
                        
                        return Ok(fileName);
                    }
                    else
                    {
                        return BadRequest(Messages.UserNotFound);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(Messages.ErrorUploadingFile);
                }
            }
            else
            {
                return BadRequest(Messages.NoFileContent);
            }
        }

        /// <summary>
        /// העלאת תמונת פרופיל 
        /// </summary>
        /// <returns></returns>
        [Route("chats/uploadVideo")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostUploadChatVideo()
        {

            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                try
                {
                    var user = base.CurrentUser;
                    if (user != null)
                    {
                        var appPath = ConfigurationManager.AppSettings["ChatUrl"];
                        var fileName = DateTime.Now.Ticks.ToString() + ".mp4";
                        var absoluteFolderPath = appPath;
                        var pathToFile = absoluteFolderPath + "\\" + fileName;

                        if (!Directory.Exists(absoluteFolderPath))
                        {
                            Directory.CreateDirectory(absoluteFolderPath);
                        }

                        var provider = new PhotoMultipartFormDataStreamProvider(absoluteFolderPath, fileName);
                        await Request.Content.ReadAsMultipartAsync(provider);

                        //using (var image = Image.FromFile(pathToFile))
                        //{
                        //    var size = image.Height >= image.Width ? image.Width : image.Height;
                        //    var paddingW = (image.Width - size) / 2;
                        //    var padding = (image.Height - size) / 2;
                        //    using (var newImage = Crop(image, new Rectangle(paddingW, padding, image.Width - paddingW * 2, image.Height - padding * 2)))
                        //    {
                        //        newImage.Save(absoluteFolderPath + "\\" + fileName, ImageFormat.Jpeg);
                        //    }
                        //}

                        return Ok(fileName);
                    }
                    else
                    {
                        return BadRequest(Messages.UserNotFound);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(Messages.ErrorUploadingFile);
                }
            }
            else
            {
                return BadRequest(Messages.NoFileContent);
            }
        }

        public static Image Crop(Image image, Rectangle selection)
        {
            var bmp = image as Bitmap;

            // Check if it is a bitmap:
            if (bmp == null)
                throw new ArgumentException("No valid bitmap");

            // Crop the image:
            var cropBmp = bmp.Clone(selection, bmp.PixelFormat);

            // Release the resources:
            image.Dispose();

            return cropBmp;
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
        public void GetPlayers(ChatUserViewModel cuvm, Team team, int? seasonId)
        {
            if (cuvm.Players == null)
                cuvm.Players = new List<ChatUserModel>();

            cuvm.Players.AddRange(
                trainingRepo.GetPlayersByTeamId(team.TeamId, seasonId)
                    .Select(t => new ChatUserModel
                    {
                        UserId = t.UserId,
                        UserName = t.User.FullName
                    })
                    .ToList()
                    .GroupBy(i => i.UserId)
                    .Select(g => g.First())
                    .Distinct()
                    .ToList());
            //cuvm.Players = cuvm.Players.GroupBy(i => i.UserId).Select(g => g.First()).Distinct().ToList();
        }
        public void GetTeamManagers(ChatUserViewModel cuvm, Team team, int? seasonId)
        {
            if (cuvm.TeamOfficials == null)
                cuvm.TeamOfficials = new List<ChatUserModel>();

            cuvm.TeamOfficials.AddRange(
                jobRepo.GetTeamUsersJobs(team.TeamId, seasonId)
                    .Where(x => x.RoleName == JobRole.TeamManager && x.UserId != this.CurrUserId)
                    .Select(t => new ChatUserModel
                    {
                        UserId = t.UserId,
                        UserName = t.FullName
                    }).ToList().GroupBy(i => i.UserId).Select(g => g.First()).ToList());
            //cuvm.TeamOfficials = cuvm.TeamOfficials.GroupBy(i => i.UserId).Select(g => g.First()).ToList();
        }
        public void GetLeagueManager(ChatUserViewModel cuvm, League league, int? seasonId)
        {
            if (cuvm.LeagueOfficials == null)
                cuvm.LeagueOfficials = new List<ChatUserModel>();

            cuvm.LeagueOfficials.AddRange(
                jobRepo.GetLeagueUsersJobs(league.LeagueId, seasonId)
                    .Where(x => x.RoleName == JobRole.LeagueManager && x.UserId != this.CurrUserId)
                    .Select(x => new ChatUserModel
                    {
                        UserId = x.UserId,
                        UserName = x.FullName
                    }).ToList());
            cuvm.LeagueOfficials = cuvm.LeagueOfficials.GroupBy(i => i.UserId).Select(g => g.First()).ToList();

            foreach (var tt in league.LeagueTeams)
            {
                this.GetTeamManagers(cuvm, tt.Teams, seasonId);
                this.GetPlayers(cuvm, tt.Teams, seasonId);
            }
        }

        public void GetClubManager(ChatUserViewModel cuvm, Club club, int? seasonId)
        {
            if (cuvm.ClubOfficials == null)
                cuvm.ClubOfficials = new List<ChatUserModel>();

            cuvm.ClubOfficials.AddRange(
                jobRepo.GetClubUsersJobs(club.ClubId, seasonId)
                    .Where(x => x.RoleName == JobRole.ClubManager && x.UserId != this.CurrUserId)
                    .Select(x => new ChatUserModel
                    {
                        UserId = x.UserId,
                        UserName = x.FullName
                    }).GroupBy(i => i.UserId).Select(g => g.FirstOrDefault()));

            //cuvm.ClubOfficials = cuvm.ClubOfficials.GroupBy(i => i.UserId).Select(g => g.First()).ToList();
            this.GetDepartmentManager(cuvm, club, seasonId);
            foreach (var tt in club.ClubTeams)
            {
                this.GetTeamManagers(cuvm, tt.Team, seasonId);
                this.GetPlayers(cuvm, tt.Team, seasonId);
            }
        }

        public void GetDepartmentManager(ChatUserViewModel cuvm, Club club, int? seasonId)
        {
            if (cuvm.DepartmentOfficials == null)
                cuvm.DepartmentOfficials = new List<ChatUserModel>();

            cuvm.DepartmentOfficials.AddRange(
                jobRepo.GetDepartUsersJobs(club, seasonId)
                    .Where(x => x.RoleName == JobRole.DepartmentManager && x.UserId != this.CurrUserId)
                    .Select(t => new ChatUserModel
                    {
                        UserId = t.UserId,
                        UserName = t.FullName
                    }).ToList().GroupBy(i => i.UserId).Select(g => g.First()).ToList());
            //cuvm.DepartmentOfficials = cuvm.DepartmentOfficials.GroupBy(i => i.UserId).Select(g => g.First()).ToList();
            var clublist = db.UsersJobs.Where(g => g.Club.ParentClubId == club.ClubId).Select(x => x.Club);
            foreach(var clubitem in clublist)
            {
                foreach (var tt in clubitem.ClubTeams)
                {
                    this.GetTeamManagers(cuvm, tt.Team, seasonId);
                    this.GetPlayers(cuvm, tt.Team, seasonId);
                }
            }
        }

        public void GetUnionManager(ChatUserViewModel cuvm, Union union, int userId, int? seasonId)
        {
            if (cuvm.UnionOfficials == null)
                cuvm.UnionOfficials = new List<ChatUserModel>();
            var unionId = union.UnionId;

            cuvm.UnionOfficials.AddRange(
                jobRepo.GetUnionUsersJobs(union.UnionId, seasonId)
                    .Where(x => x.RoleName == JobRole.UnionManager && x.UserId == userId && x.UserId != this.CurrUserId)
                    .Select(x => new ChatUserModel
                    {
                        UserId = x.UserId,
                        UserName = x.FullName,
                    }).ToList());

            cuvm.UnionOfficials = cuvm.UnionOfficials.GroupBy(i => i.UserId).Select(g => g.First()).ToList();

            var leagues = union.Leagues.Where(l => /* l.EilatTournament == null && */ !l.IsArchive && l.SeasonId == seasonId);

            foreach (var tt in leagues)
            {
                this.GetLeagueManager(cuvm, tt, seasonId);
            }

            var clubs = union.Clubs.Where(x => x.UnionId == unionId && x.SeasonId == seasonId && x.IsArchive == false).ToList();

            foreach (var club in clubs)
            {
                this.GetClubManager(cuvm, club, seasonId);
            }
        }

        [Route("gallery/{type}/{id}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostUploadGalleryImage(string type, int id)
        {

            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                try
                {
                    var user = base.CurrentUser;
                    if (user != null)
                    {
                        var appPath = ConfigurationManager.AppSettings["TeamUrl"];

                        switch (type)
                        {
                            case "league":
                                appPath = ConfigurationManager.AppSettings["LeagueUrl"];
                                break;
                            case "club":
                                appPath = ConfigurationManager.AppSettings["ClubUrl"];
                                break;
                        }

                        var fileName = user.UserId.ToString() + "__" + DateTime.Now.Ticks.ToString() + ".jpeg";
                        var absoluteFolderPath = appPath + "\\" + id.ToString();
                        var pathToFile = absoluteFolderPath + "\\" + fileName;

                        if (!Directory.Exists(absoluteFolderPath))
                        {
                            Directory.CreateDirectory(absoluteFolderPath);
                        }

                        var provider = new PhotoMultipartFormDataStreamProvider(absoluteFolderPath, fileName);
                        await Request.Content.ReadAsMultipartAsync(provider);

                        //using (var image = Image.FromFile(pathToFile))
                        //{
                        //    var size = image.Height >= image.Width ? image.Width : image.Height;
                        //    var paddingW = (image.Width - size) / 2;
                        //    var padding = (image.Height - size) / 2;
                        //    using (var newImage = Crop(image, new Rectangle(paddingW, padding, image.Width - paddingW * 2, image.Height - padding * 2)))
                        //    {
                        //        newImage.Save(absoluteFolderPath + "\\" + fileName, ImageFormat.Jpeg);
                        //    }
                        //}

                        return Ok(fileName);
                    }
                    else
                    {
                        return BadRequest(Messages.UserNotFound);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(Messages.ErrorUploadingFile);
                }
            }
            else
            {
                return BadRequest(Messages.NoFileContent);
            }
        }

        public class TokenItem
        {
            public string Token { get; set; }
            public bool IsIOS { get; set; }
            public string Section { get; set; }
        }

        public class ChatViewModel
        {
            public int MsgId { get; set; }
            public string Message { get; set; }
            public DateTime SendDate { get; set; }
            public bool IsRead { get; set; }
            public int? SenderId { get; set; }
            public string SenderName { get; set; }
            public string SenderImage { get; set; }
            public string img { get; set; }
            public string video { get; set; }
            // Cheng Li. : Add url of image, video
            public string imgUrl { get; set; }
            public string videoUrl { get; set; }
            public virtual List<CommentViewModel> Childs { get; set; }
            public virtual List<ReceiveModel> Receives { get; set; }
        }
        // Cheng Li.ReceiveNames Model.
        public class ReceiveModel
        {
            public int receiveId { get; set; }
            public string receiveName { get; set; }
            public string shortName { get; set; }
        }

        public class CommentViewModel : ChatViewModel
        {
            public int? parent { get; set; }
        }

        public class PostChatMessage
        {
            public List<int> Users { get; set; }
            public string Message { get; set; }
            public int? SenderId { get; set; }
            public int? parent { get; set; }
            public string img { get; set; }
            public string video { get; set; }
        }

        public class ChatUserViewModel
        {
            public List<ChatUserModel> Friends { get; set; }
            public List<ChatUserModel> UnionOfficials { get; set; }
            public List<ChatUserModel> LeagueOfficials { get; set; }
            public List<ChatUserModel> ClubOfficials { get; set; }
            public List<ChatUserModel> DepartmentOfficials { get; set; }
            public List<ChatUserModel> TeamOfficials { get; set; }
            public List<ChatUserModel> Teams { get; set; }
            public List<ChatUserModel> Players { get; set; }
            // Cheng Li. Add
            public List<ChatUserForTeamViewModel> TeamUsers { get; set; }
            public string JobRole { get; set; }
            public string GameName { get; set; }
        }

        // Cheng Li. Add
        public class ChatUserForTeamViewModel
        {

            public int? TeamId { get; set; }
            public string TeamName { get; set; }
            public bool IsSelectAll { get; set; }
            public List<ChatUserModel> Users { get; set; }
            public List<ChatUserModel> TeamOfficials { get; set; }
            public List<ChatUserModel> Players { get; set; }
        }

        public class ChatUserModel
        {
            public int UserId { get; set; }
            public string UserName { get; set; }

            // Cheng Li. add UserRole
            public string UserRole { get; set; }
        }

        public class ChatTeamModel
        {
            public int TeamId { get; set; }
            public string TeamName { get; set; }
        }

        public class PostForwardModel
        {
            public int MsgId { get; set; }
            public int[] Friends{ get; set; }
        }
    }
}
