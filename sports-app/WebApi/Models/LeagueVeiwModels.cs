using AppModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataService.LeagueRank;
using DataService.DTO;
using DataService;
namespace WebApi.Models
{

    public class LeagueInfoVeiwModel
    {

        public LeagueInfoVeiwModel() { }

        public LeagueInfoVeiwModel(League league)
        {

            Id = league.LeagueId;
            AthleticId = league.AthleticLeagueId ?? 0;
            docId = 0;
            Logo = league.Logo;
            Image = league.Image;
            Title = league.Name;
            AboutLeague = league.AboutLeague;
            LeagueStructure = league.LeagueStructure;
            Type = 0;
            PlaceOfCompetition = league.PlaceOfCompetition;
            RegistrationLink = league.RegistrationLink;

            // Cheng Li. : set date, discipline field 
            if (league.LeagueStartDate != null)
                LeagueStartDate = (DateTime)league.LeagueStartDate;
            if (league.LeagueEndDate != null)
                LeagueEndDate = (DateTime)league.LeagueEndDate;
            if (league.EndRegistrationDate != null)
                EndRegistrationDate = (DateTime)league.EndRegistrationDate;
            if (league.PlaceOfCompetition != null)
                Address = league.PlaceOfCompetition;
            if (league.MaximumAge != null)
                MaximumAge = (DateTime)league.MaximumAge;
            if (league.MinimumAge != null)
                MinimumAge = (DateTime)league.MinimumAge;
            if (league.Discipline != null)
                Discipline = league.Discipline.Name;
            if (league.Type != null)
                Type = (int)league.Type;
        }

        public int Id { get; set; }

        public int AthleticId { get; set; }
        public int docId { get; set; }

        public string Logo { get; set; }
        public string Address { get; set; }

        public string Image { get; set; }

        public string Title { get; set; }

        public DateTime EndRegistrationDate { get; set; }

        public string Discipline { get; set; }

        public DateTime LeagueStartDate { get; set; }

        public DateTime LeagueEndDate { get; set; }

        /** Cheng Li. : Add Variable : AboutLeague, LeagueStructure, MaximumAge, Type, PlaceOfCompetition */
        public string AboutLeague { get; set; }

        public string LeagueStructure { get; set; }

        public DateTime MaximumAge { get; set; }

        public DateTime MinimumAge { get; set; }

        // Type 0:League 1:Competition, 2:...
        public int Type { get; set; }

        public string PlaceOfCompetition { get; set; }
        public string RegistrationLink { get; set; }
    }

    public class LeaguePageVeiwModel
    {

        public LeagueInfoVeiwModel LeagueInfo { get; set; }
        public LevelDateSetting LevelDateSetting { get; set; }
        public List<AthleticsLeagueStandingModel> CompetitionRanks { get; set; }
        public List<CompetitionClubCorrectionDTO> LeagueRanks { get; set; }
        public TeamCompactViewModel TeamWithMostFans { get; set; }

        public NextGameViewModel NextGame { get; set; }

        public IEnumerable<GameViewModel> NextGames { get; set; }

        public IEnumerable<GameViewModel> LastGames { get; set; }

        public List<RankStage> LeagueTableStages { get; set; }
        public List<CompetitionDisciplineDto> CompetitionDisciplines { get; set; }

        public List<int> GameCycles { get; set; }

        // Cheng Li Add: Get Teams Info
        public CompetitionTeamViewModel Team { get; set; }

        public IEnumerable<BasicRouteViewModel> Routes { get; set; }
        public string SectionName { get; set; }
    }

    //public class LeagueTableVeiwModel
    //{

    //    public LeagueInfoVeiwModel LeagueInfo { get; set; }


    //    public List<DataService.LeagueRank.RankStage> Stages { get; set; }
    //}
    public class CompetitionItemViewModel
    {
        public List<LeaguesListItemViewModel> categoriLIstItem { get; set; }
        public int categoriNum { get; set; }
        public int categoriPlayersNum { get; set; }
        public string CompetitionTitle { get; set; }
        public string Logo { get; set; }
    }
    public class LeaguesListItemViewModel
    {

        public int Id { get; set; }
        public int AthleticId { get; set; }

        public string Title { get; set; }
        public string Address { get; set; }

        public int TotalTeams { get; set; }
        public int TotalDisciplines { get; set; }
        public int TotalFans { get; set; }
        public int TotalPlayers { get; set; }
        public IEnumerable<int> FansIds { get; set; }

        public string Logo { get; set; }
        public string DisciplineName { get; set; }
        public int TeamId { get; set; }
        public int order { get; set; }
        public string StartDate { get; set; }
        public DateTime StartDate_datetime { get; set; }
    }

    public class DisciplinesListItemViewModel
    {
        public int Id { get; set; }
        public String Title { get; set; }
    }

    // Cheng Li: add for route

    public class RegistrationModel
    {
        public int UserId { get; set; }
        public int ClubId { get; set; }
        public string FullName { get; set; }
        public int? Rank { get; set; }
        public string FinalScore { get; set; }
        public string ClubName { get; set; }

    }
    public class RegisteredCompetitionModel
    {
        public string Header1 { get; set; }
        public string Header2 { get; set; }
        public string Header3 { get; set; }
        public string Logo { get; set; }
        public string IsraeliRecord { get; set; }
        public string CompetitionRecord { get; set; }
        public string IntentionalIsraeliRecord { get; set; }
        public string SeasonRecord { get; set; }
        public bool isCompetitionRecord { get; set; }
        public bool isDisciplineRecord { get; set; }
        public int Format { get; set; }
        public bool IsCombinedDiscipline { get; set; }
        public bool IsOneRecordHasValue { get; set; }
        public bool IsAnyAttempt { get; set; }
        public List<SItem> items { get; set; }
        public List<SItem> items1 { get; set; }
        public List<STItem> stitems { get; set; }
        public List<string> Cols { get; set; }
        public List<SubItem> subitems { get; set; }
    }
    public class SubItem{
        public string item1 { get; set; }
        public string item2 { get; set; }
        public string item3 { get; set; }
    }
    public class STItem
    {
        public string key { get; set; }
        public List<SItem> sitems { get; set; }
    }
    public class SItem
    {
        public string BirthDay { get; set; }
        public string ClubName { get; set; }
        public string FullName { get; set; }
        public string AthleteNumber { get; set; }
        public string Result { get; set; }
        public int UserId { get; set; }
        public string Heat { get; set; }
        public string Lane { get; set; }
        public string SB { get; set; }
        public int SeasonId { get; set; }
        public int Rank { get; set; }
        public string Wind { get; set; }
        public string Points { get; set; }
        public string Records { get; set; }
        public string Result1 { get; set; }
        public string Wind1 { get; set; }
        public string Result2 { get; set; }
        public string Wind2 { get; set; }
        public string Result3 { get; set; }
        public string Wind3 { get; set; }
        public string Result4 { get; set; }
        public string Wind4 { get; set; }
        public string Result5 { get; set; }
        public string Wind5 { get; set; }
        public string Result6 { get; set; }
        public string Wind6 { get; set; }
        public string FinalResult1 { get; set; }
        public string FinalResult2 { get; set; }
        public List<SubItem> subitems { get; set; }
    }
    public class StartListModel
    {
        public int x { get; set; }
    }
    public class AthleticsDisciplineResultsByHeatModel
    {
        public int x { get; set; }
    }
    public class AthleticsDisciplineResultsModel
    {
        public int x { get; set; }
    }

    public class TennisPlayoffRankModel
    {
        public int? Id { get; set; }
        public int? Rank { get; set; }
        public int? Points { get; set; }
        public int? UserId { get; set; }
        public string PlayerName { get; set; }
    }

    public class BaseNameModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class RouteNameModel
    {
        public int Id { get; set; }
        public string RouteName { get; set; }
    }
    public class RankNameModel
    {
        public int Id { get; set; }
        public string RankName { get; set; }
    }


    public class BasicRouteViewModel
    {
        public int Id { get; set; }
        public int DiciplineId { get; set; }
        public int? SeasonId { get; set; }
        public int LeagueId { get; set; }
        public int RouteId { get; set; }
        public int RankId { get; set; }
        public string RouteName { get; set; }
        public string RankName { get; set; }
        public int GymnasticsCount { get; set; }
        public int CompetitionRouteId { get; internal set; }
        public string DisciplineName { get; set; }
        public int? Composition { get; set; }
        public IEnumerable<int> InstrumentsIds { get; set; }
        public string InstrumentName { get; set; }
        public int? SecondComposition { get; set; }
        public bool IsCompetitiveEnabled { get; set; }
        public IEnumerable<RegistrationModel> CompetitionRegistration { get; set; }
        }
}
