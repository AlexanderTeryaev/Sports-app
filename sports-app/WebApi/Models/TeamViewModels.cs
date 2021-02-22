using AppModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataService.LeagueRank;
using WebApi.Services;

namespace WebApi.Models
{
    public class LeagueTeamsViewModel
    {
        public int LeagueId { get; set; }

        public string Name { get; set; }

        public IEnumerable<TeamCompactViewModel> Teams { get; set; }
    }

    public class ActivityCustomPriceModel
    {
        public string PropertyName { get; set; }
        public string TitleEng { get; set; }
        public string TitleHeb { get; set; }
        public string TitleUk { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Paid { get; set; }
        public string CardComProductId { get; set; }
    }

    public class TeamRegisterBindingModel
    {

        public int TeamId { get; set; }

        public string Title { get; set; }

        public int LeagueId { get; set; }

    }

    public class FanTeamsViewModel
    {

        public int TeamId { get; set; }

        public string Title { get; set; }

        public int LeagueId { get; set; }

    }

    // Cheng Li: Add TeamViewModelForCompetition
    public class CompetitionTeamViewModel
    {
        public CompetitionTeamViewModel(DataEntities db, Team team, int unionId, int leagueId, int currentUserId, int? seasonId)
        {
            TeamId = team.TeamId;
            Title = team.Title;
            Description = team.Description;
            CreateDate = team.CreateDate;
            MaximumAge = team.MaximumAge;
            MinimumAge = team.MinimumAge;
            PlaceForQualification = team.PlaceForQualification;
            PlaceForFinal = team.PlaceForFinal;

            CompetitionAge age = db.CompetitionAges.FirstOrDefault(x => x.id == team.CompetitionAgeId && x.UnionId == unionId);

            if(age != null)
            {
                Gender = db.Genders.Where(x => x.GenderId == age.gender).FirstOrDefault().TitleMany;
            }
            else
            {
                Gender = "";
            }
            Level = db.CompetitionLevels.Where(x => x.id == team.LevelId).FirstOrDefault().level_name;
            Region = db.CompetitionRegions.Where(x => x.id == team.RegionId).FirstOrDefault().region_name;

            Players = seasonId != null ? PlayerService.GetActivePlayersByTeamId(team.TeamId, (int)seasonId, leagueId) : new List<CompactPlayerViewModel>();
            var playersPaid = new List<CompactPlayerViewModel>();
            foreach (var player in Players)
            {
                var isAtleastOnePaid = false;
                foreach (var activityForm in player.ActivityForms.Where(a => a.TeamId == team.TeamId))
                {
                    var activity = db.Activities.FirstOrDefault(a => a.ActivityId == activityForm.ActivityId);
                    activityForm.Activity = activity;
                    decimal balance = 0;
                    balance += activityForm.Activity.RegistrationPrice ? (!activityForm.IsPaymentByBenefactor ? activityForm.RegistrationPrice : 0) : 0;
                    balance += activityForm.Activity.InsurancePrice ? (activityForm.DisableInsurancePayment == true ? 0 : activityForm.InsurancePrice) : 0;
                    balance += activityForm.Activity.MembersFee ? (activityForm.DisableMembersFeePayment == true ? 0 : activityForm.MembersFee) : 0;
                    balance += activityForm.Activity.HandlingFee ? (activityForm.DisableHandlingFeePayment == true ? 0 : activityForm.HandlingFee) : 0;

                    var customPrices =  !string.IsNullOrWhiteSpace(activityForm.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(activityForm.CustomPrices)
                                        : new List<ActivityCustomPriceModel>();

                    balance += activityForm.Activity.CustomPricesEnabled ? customPrices.Sum(x => x.TotalPrice) : 0;

                    balance -= activityForm.Activity.RegistrationPrice ? (!activityForm.IsPaymentByBenefactor ? activityForm.RegistrationPaid : 0) : 0;
                    balance -= activityForm.Activity.InsurancePrice ? (activityForm.DisableInsurancePayment ? activityForm.InsurancePaid : 0) : 0;
                    balance -= activityForm.Activity.MembersFee ? (activityForm.DisableMembersFeePayment == true ? 0 : activityForm.MembersFeePaid) : 0;
                    balance -= activityForm.Activity.HandlingFee ? (activityForm.DisableHandlingFeePayment == true ? 0 : activityForm.HandlingFeePaid) : 0;
                    balance -= activityForm.Activity.CustomPricesEnabled ? customPrices.Sum(x => x.Paid) : 0;


                    if (balance <= 0)
                    {
                        isAtleastOnePaid = true;
                        break;
                    }
                }
                if (!isAtleastOnePaid)
                {
                    foreach (var activityForm in player.ActivityForms.Where(a => a.LeagueId == leagueId))
                    {
                        var activity = db.Activities.FirstOrDefault(a => a.ActivityId == activityForm.ActivityId);
                        activityForm.Activity = activity;
                        decimal balance = 0;
                        balance += activityForm.Activity.RegistrationPrice ? (!activityForm.IsPaymentByBenefactor ? activityForm.RegistrationPrice : 0) : 0;
                        balance += activityForm.Activity.InsurancePrice ? (activityForm.DisableInsurancePayment == true ? 0 : activityForm.InsurancePrice) : 0;
                        balance += activityForm.Activity.MembersFee ? (activityForm.DisableMembersFeePayment == true ? 0 : activityForm.MembersFee) : 0;
                        balance += activityForm.Activity.HandlingFee ? (activityForm.DisableHandlingFeePayment == true ? 0 : activityForm.HandlingFee) : 0;

                        var customPrices = !string.IsNullOrWhiteSpace(activityForm.CustomPrices)
                                            ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(activityForm.CustomPrices)
                                            : new List<ActivityCustomPriceModel>();

                        balance += activityForm.Activity.CustomPricesEnabled ? customPrices.Sum(x => x.TotalPrice) : 0;

                        var totalFee = balance;
                        decimal totalPaid = 0;
                        totalPaid += activityForm.Activity.RegistrationPrice ? (!activityForm.IsPaymentByBenefactor ? activityForm.RegistrationPaid : 0) : 0;
                        totalPaid += activityForm.Activity.InsurancePrice ? (activityForm.DisableInsurancePayment ? activityForm.InsurancePaid : 0) : 0;
                        totalPaid += activityForm.Activity.MembersFee ? (activityForm.DisableMembersFeePayment == true ? 0 : activityForm.MembersFeePaid) : 0;
                        totalPaid += activityForm.Activity.HandlingFee ? (activityForm.DisableHandlingFeePayment == true ? 0 : activityForm.HandlingFeePaid) : 0;
                        totalPaid += activityForm.Activity.CustomPricesEnabled ? customPrices.Sum(x => x.Paid) : 0;


                        if (totalFee-totalPaid <= 0 && totalFee > 0)
                        {
                            isAtleastOnePaid = true;
                            break;
                        }
                    }
                }
                if (isAtleastOnePaid)
                {
                    playersPaid.Add(player);
                }
            }

            playersPaid = playersPaid.OrderBy(x => x.FullName).ToList();

            var test = Players.OrderBy(x => x.FullName).ToList();
            var test2 = test.Except(playersPaid);
            if (test2.Count() > 0)
            {
                var test4 = true;
            }
            Players = playersPaid;
            // Set friends status for each of the players
            FriendsService.AreFriends(Players, currentUserId);
            //Jobs
            Jobs = TeamsService.GetTeamJobsByTeamId(team.TeamId, currentUserId, seasonId);
        }

        public int TeamId { get; set; }
        public string Title { get; set; }
        public string Logo { get; set; }
        public string PersonnelPic { get; set; }
        public string Description { get; set; }
        public string OrgUrl { get; set; }
        public System.DateTime CreateDate { get; set; }
        public bool IsArchive { get; set; }
        public string GamesUrl { get; set; }
        public Nullable<bool> IsUnderAdult { get; set; }
        public Nullable<bool> IsReserved { get; set; }
        public Nullable<bool> NeedShirts { get; set; }
        public string InsuranceApproval { get; set; }
        public Nullable<bool> HasArena { get; set; }
        public bool IsTrainingEnabled { get; set; }
        public Nullable<System.DateTime> MinimumAge { get; set; }
        public Nullable<System.DateTime> MaximumAge { get; set; }
        public Nullable<int> CompetitionAgeId { get; set; }
        public string Gender { get; set; }
        public string Region { get; set; }
        public string Level { get; set; }
        public Nullable<int> MinRank { get; set; }
        public Nullable<int> MaxRank { get; set; }
        public string PlaceForQualification { get; set; }
        public string PlaceForFinal { get; set; }
        public bool IsReligiousTeam { get; set; }
        public Nullable<bool> IsUnionInsurance { get; set; }
        public Nullable<bool> IsClubInsurance { get; set; }
        public List<CompactPlayerViewModel> Players { get; set; }
        public List<TeamJobsViewModel> Jobs { get; set; }
    }

    public class TeamCompactViewModel
    {
        public TeamCompactViewModel(Team team, int leagueId, int? seasonId = null)
        {
            TeamId = team.TeamId;
            LeagueId = leagueId;
            Logo = team.Logo;

            if (seasonId.HasValue)
            {
                TeamsDetails teamsDetails = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId);
                Title = teamsDetails != null ? teamsDetails.TeamName : team.Title;
            }
            else
            {
                Title = team.Title;
            }
        }

        public TeamCompactViewModel()
        {
            // TODO: Complete member initialization
        }

        public int TeamId { get; set; }

        public string Title { get; set; }

        public int LeagueId { get; set; }

        public string Logo { get; set; }

        public int FanNumber { get; set; }
    }


    public class TeamPageViewModel
    {

        public TeamInfoViewModel TeamInfo { get; set; }

        public NextGameViewModel NextGame { get; set; }

        public GameViewModel LastGame { get; set; }

        public IEnumerable<LeagueInfoVeiwModel> Leagues { get; set; }

        public List<UserBaseViewModel> Fans { get; set; }

        public List<int> GameCycles { get; set; }

        public List<CompactPlayerViewModel> Players { get; set; }

        public IEnumerable<GameViewModel> NextGames { get; set; }

        public IEnumerable<GameViewModel> LastGames { get; set; }

        public List<TeamJobsViewModel> Jobs { get; set; }
        public List<WallThreadViewModel> MessageThreads { get; internal set; }
        public List<RankStage> LeagueTableStages { get; set; }
        public string SectionName { get; set; }
        public int LeagueId { get; set; }
    }

    public class ClubTeamPageViewModel
    {
        public TeamInfoViewModel TeamInfo { get; set; }
        public int TeamId { get; set; }
        public int ClubId { get; set; }

        public string SectionName { get; set; }
        public string Ratio { get; set; }

        public int SuccsessLevel { get; set; }

        public int Place { get; set; }

        public string Logo { get; set; }

        public string Image { get; set; }

        public string Title { get; set; }

        public List<TeamStandingsForm> TeamStandings { get; set; }

        public List<UserBaseViewModel> Fans { get; set; }

        public List<CompactPlayerViewModel> Players { get; set; }

        public List<TeamJobsViewModel> Jobs { get; set; }

        public List<WallThreadViewModel> MessageThreads { get; internal set; }

        public List<ClubGame> LastGames { get; set; }

        public List<ClubGame> NextGames { get; set; }

        public NextGameViewModel NextGame { get; set; }

        public GameViewModel LastGame { get; set; }
        public string Description { get; set; }
    }

    public partial class ClubGame
    {
        public int GameId { get; set; }
        public System.DateTime StartDate { get; set; }
        public string HomeTeam { get; set; }
        public int HomeTeamId { get; set; }
        public string HomeTeamScore { get; set; }
        public string HomeTeamLogo { get; set; }

        public string GuestTeam { get; set; }
        public int GuestTeamId { get; set; }
        public string GuestTeamScore { get; set; }
        public string GuestTeamLogo { get; set; }

        public string Score { get; set; }
        public string Auditorium { get; set; }
        public int SchedulerScrapperGamesId { get; set; }
        public string Status { get; set; }
        public int IsGoing { get; set; }
    }

    public class TeamInfoViewModel
    {

        public int TeamId { get; set; }

        public int Place { get; set; }

        public string Ratio { get; set; }

        public int SuccsessLevel { get; set; }

        public string Logo { get; set; }

        public string Image { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        public string League { get; set; }
        public string Club { get; set; }

        public int LeagueId { get; set; }
        public int ClubId { get; set; }
        public string SectionName { get; set; }
    }

    public class TeamJobsViewModel
    {

        public int Id { get; set; }

        public string JobName { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; }

        //[JsonIgnore]
        //public DateTime? BirthDay { get; set; }

        //public int? Age
        //{
        //    get
        //    {
        //        if (this.BirthDay.HasValue)
        //        {
        //            DateTime zeroTime = new DateTime(1, 1, 1);
        //            DateTime a = this.BirthDay.Value;
        //            DateTime b = DateTime.Now;
        //            TimeSpan span = b - a;
        //            int years = (zeroTime + span).Year - 1;
        //            return years;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        public bool IsFriend { get; set; }

        public string FriendshipStatus { get; set; }
    }

    //public class TeamProfileVeiwModel
    //{

    //    public TeamProfileVeiwModel()
    //    {
    //        Players = new List<CompactPlayerViewModel>();

    //        Fans = new List<FanCompactViewModel>();
    //    }

    //    public int TeamId { get; set; }

    //    public string Title { get; set; }

    //    public string Logo { get; set; }

    //    public string PersonnelPic { get; set; }

    //    public string Description { get; set; }

    //    public string OrgUrl { get; set; }

    //    public DateTime CreateDate { get; set; }

    //    public virtual List<CompactPlayerViewModel> Players { get; set; }

    //    public virtual List<FanCompactViewModel> Fans { get; set; }
    //}

}