using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using DataService;
using WebApi.Models;
using WebApi.Services;
using System;
using System.Data.Entity;
using System.Linq;
using AppModel;

namespace WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [AllowAnonymous]
    [RoutePrefix("api/Training")]
    public class TrainingController : BaseLogLigApiController
    {
        TeamTrainingsRepo _objTeamTrainingsRepo = new TeamTrainingsRepo();
        readonly SeasonsRepo _seasonsRepo = new SeasonsRepo();
        /// <summary>
        /// Get training info about sepcific team by his id
        /// </summary>
        /// <param name="teamId">id of team</param>
        /// <returns></returns>
        [Route("Teams/{teamId}")]
        [ResponseType(typeof(TeamTrainingsViewModel))]
        public IHttpActionResult GetTeamTraininngs(int teamId)
        {
            if( CurrentUser == null ) return null;
            var currentUserId = CurrentUser.UserId;
            var seasonId = _seasonsRepo.GetLastSeasonIdByCurrentTeamId(teamId);
            var teamsTrainings = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(teamId, seasonId);
            var vm = new TeamTrainingsViewModel();
            vm.NextTrainings = new List<TeamTrainingViewModel>();
            vm.PrevTrainings = new List<TeamTrainingViewModel>();
            TeamTraining next = null;
            foreach (var training in teamsTrainings)
            {
                if (!training.isPublished)
                    continue;
                var teamTraining = new TeamTrainingViewModel
                {
                    Id = training.Id,
                    Title = training.Title,
                    TeamId = training.TeamId,
                    TeamName = training.Team.Title,
                    AuditoriumId = training.AuditoriumId,
                    Arena = training.Auditorium.Name,
                    Content = training.Content,
                    TrainingDate = training.TrainingDate
                };

                if (training.TrainingDate > DateTime.Now)
                {
                    if (next == null)
                        next = training;
                    vm.NextTrainings.Add(teamTraining);
                }
                else
                    vm.PrevTrainings.Add(teamTraining);
            }
            
            vm.NextTraining = ParseNextTraining(currentUserId, next);
            if (vm.NextTraining != null)
                vm.NextTraining.HomeTeamId = teamId;

            return Ok(vm);
        }

        public NextGameViewModel ParseNextTraining(int currentUserId, TeamTraining training)
        {
            if (training != null)
            {
                IEnumerable<UserBaseViewModel> FansList = GetTrainingFans(currentUserId, training);
                var me = FansList.FirstOrDefault(t => t.Id == currentUserId);
                var result = new NextGameViewModel
                {
                    GameId = training.Id,
                    StartDate = training.TrainingDate,
                    Auditorium = training.Auditorium.Address,
                    FansList = FansList.ToList(),
                    FriendsGoing = FansList.Count(t => t.FriendshipStatus == FriendshipStatus.Yes),
                    FansGoing = FansList.Count(t => t.FriendshipStatus != FriendshipStatus.Yes),
                    IsGoing = me != null ? 1 : 0,
                    GroupName = training.Title,
                    HomeTeam = training.Content
                };
                return result;
            }
            return null;
        }

        public List<UserBaseViewModel> GetTrainingFans(int currentUserId, TeamTraining training)
        {
            var result = new List<UserBaseViewModel>();
            foreach (var gu in training.TrainingAttendances)
            {
                foreach (var us in gu.TeamsPlayer.User.Friends.Where(t => t.FriendId == currentUserId && t.UserId == gu.TeamsPlayer.UserId).DefaultIfEmpty())
                {
                    foreach (var usf in gu.TeamsPlayer.User.UsersFriends.Where(t => t.UserId == currentUserId && t.FriendId == gu.TeamsPlayer.UserId).DefaultIfEmpty())
                    {
                        if (gu.TeamsPlayer.User.UsersType.TypeRole == "players")
                        {
                            var ppv = PlayerService.GetPlayerProfile(gu.TeamsPlayer.User, null);
                            if (gu.TeamsPlayer.User.Image == null)
                            {
                                gu.TeamsPlayer.User.Image = ppv.Image;
                            }
                        }

                        result.Add(new UserBaseViewModel
                            {
                                Id = gu.TeamsPlayer.UserId,
                                UserName = gu.TeamsPlayer.User.UserName,
                                FullName = gu.TeamsPlayer.User.FullName,
                                Image = gu.TeamsPlayer.User.Image,
                                CanRcvMsg = true,
                                UserRole = gu.TeamsPlayer.User.UsersType.TypeRole,
                                FriendshipStatus = (us == null && usf == null)
                                    ? FriendshipStatus.No
                                    :
                                    (us != null && us.IsConfirmed) || (usf != null && usf.IsConfirmed)
                                        ?
                                        FriendshipStatus.Yes
                                        :
                                        FriendshipStatus.Pending
                            }
                        );
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get training info about sepcific training id
        /// </summary>
        /// <param name="teamId">id of team</param>
        /// <param name="trainingId">id of team</param>
        /// <returns></returns>
        [Route("Teams/{teamId}/Trainings/{trainingId}")]
        [ResponseType(typeof(TeamTrainingAttendanceViewModel))]
        public IHttpActionResult GetTeamTraininng(int teamId, int trainingId, int? unionId = null)
        {
            //var teamIdCurrent = data.TeamId == 0
            //    ? data.TeamId = db.TeamTrainings.Where(t => t.Id == data.Id).FirstOrDefault().TeamId
            //    : data.TeamId;
            var seasonId = unionId != null ? _seasonsRepo.GetCurrentByUnionId(unionId.Value) : (int?)null;
            var vm = new TeamTrainingAttendanceViewModel();
            var teamTraining = _objTeamTrainingsRepo
                .GetAllTeamTrainingsByTeamId(teamId, seasonId)
                .FirstOrDefault(t => t.Id == trainingId);
            vm.Id = trainingId;
            vm.Title = teamTraining.Title;
            vm.Content = teamTraining.Content;
            vm.TrainingDate = teamTraining.TrainingDate;
            vm.TeamsPlayers = new Dictionary<int, string>();
            var players = _objTeamTrainingsRepo.GetPlayersByTeamId(teamId);
            foreach (var pp in players)
            {
                if(_seasonsRepo.isNowAllowSeasonId((int)pp.SeasonId))
                {
                    vm.TeamsPlayers.Add(pp.Id, pp.User.FullName);
                }
            }
            vm.SelectedPlayers = db.TrainingAttendances.Where(t => t.TrainingId == trainingId)
                    .Select(t => t.PlayerId).ToList();

            return Ok(vm);
        }

        [Route("UpdateTeamTraining")]
        [HttpPost]
        public IHttpActionResult UpdateTeamTraining(PostTrainingAttendanceData data)
        {
            var seasonId = _seasonsRepo.GetLastSeasonIdByCurrentTeamId(data.TeamId);
            var teamTraining = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(data.TeamId, seasonId)
                .Where(t => t.Id == data.Id).FirstOrDefault();

            _objTeamTrainingsRepo.UpdateTeamTrainingById(data.Id, teamTraining.Title, teamTraining.AuditoriumId, teamTraining.TrainingDate, teamTraining.Content, data.playersId, null);
            return Ok();
        }

        [Route("NewTrialPlayer")]
        [HttpPost]
        public IHttpActionResult NewTrialPlayer(NewTrialPlayerModel data)
        {
            //TODO: we should get rid of "name"(FullName) from request and use First, Last and Middle names ASAP
            var playersRepo = new PlayersRepo(db);
            var firstName = playersRepo.GetFirstNameByFullName(data.name);
            var lastName = playersRepo.GetLastNameByFullName(data.name);
            /////////////////////////////////

            _objTeamTrainingsRepo.CreateTrialPlayer(data.teamId, data.clubId, data.unionId,
                firstName,
                lastName,
                string.Empty, //TODO: no MiddleName yet
                data.phone,
                CurrUserId);
            return Ok();
        }

        // POST: api/Training/Going
        [Route("Going")]
        public IHttpActionResult PostFanGoing(GoingTrainingDTO item)
        {
            var user = CurrentUser;
            var player = db.TeamsPlayers.Where(p => p.UserId == user.UserId).FirstOrDefault();
            var seasonId = _seasonsRepo.GetLastSeasonIdByCurrentTeamId(item.TeamId);
            var teamTraining = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(item.TeamId, seasonId)
               .Where(t => t.Id == item.Id).FirstOrDefault();

            var selectedPlayers = db.TrainingAttendances.Where(t => t.TrainingId == item.Id)
                    .Select(t => t.PlayerId).ToList();

            if (item.IsGoing == 1)
            {
                if (selectedPlayers.IndexOf(player.Id) == -1)
                    selectedPlayers.Add(player.Id);
            }
            else
            {
                if (selectedPlayers.IndexOf(player.Id) != -1)
                    selectedPlayers.Remove(player.Id);
            }

            var selectIds = new List<String>();
            foreach (var pp in selectedPlayers)
            {
                selectIds.Add(pp.ToString());
            }

            _objTeamTrainingsRepo.UpdateTeamTrainingById(teamTraining.Id, teamTraining.Title, teamTraining.AuditoriumId, teamTraining.TrainingDate, teamTraining.Content, selectIds, null);
            
            return Ok();
        }
        public class TeamTrainingViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int TeamId { get; set; }
            public string TeamName { get; set; }
            public int? AuditoriumId { get; set; }
            public String Arena { get; set; }
            public DateTime TrainingDate { get; set; }
            public string Content { get; set; }
            //public bool isPublished { get; set; }
            //public List<UserBaseViewModel> Players { get; set; }
            //public IEnumerable<AppModel.TeamsPlayer> Players { get; set; }
            //public Dictionary<int, List<int>> PlayerAttendance { get; set; }
            //public int ClubPosition { get; set; }
            //public DayOfWeek TrainingDay { get; set; }
            //public TimeSpan TrainingStartTime { get; set; }
            //public TimeSpan TrainingEndTime { get; set; }
        }

        public class TeamTrainingsViewModel
        {
            public List<TeamTrainingViewModel> NextTrainings { get; set; }
            public List<TeamTrainingViewModel> PrevTrainings { get; set; }
            public NextGameViewModel NextTraining { get; set; }
        }

        public class TeamTrainingAttendanceViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public DateTime TrainingDate { get; set; }
            public Dictionary<int, String> TeamsPlayers { get; set; }
            public List<int> SelectedPlayers { get; set; }
        }

        public class PostTrainingAttendanceData
        {
            public int Id { get; set; }
            public int TeamId { get; set; }
            public IEnumerable<string> playersId { get; set; }
        }
        public class NewTrialPlayerModel
        {
            public int teamId { get; set; }
            public int clubId { get; set; }
            public int unionId { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
        }

        public class GoingTrainingDTO
        {
            public int Id { get; set; }
            public int TeamId { get; set; }
            public int IsGoing { get; set; }
        }
    }
}
