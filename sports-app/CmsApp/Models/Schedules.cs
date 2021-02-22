using AppModel;
using DataService;
using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class Schedules
    {
        public class DateFilterPeriod
        {
            public const int BeginningOfMonth = 0;
            public const int Ranged = 1;
            public const int All = 2;
            public const int FromToday = 3;
        }
        public int? UnionId { get; set; }
        public IList<ScheduleGroup> Groups { get; set; }
        public IList<GameInLeague> Games { get; set; }
        public List<AuditoriumShort> Auditoriums { get; set; }
        public LeagueShort[] Leagues { get; set; }
        //set FromToday as default option
        public int dateFilterType { get; set; } = DateFilterPeriod.FromToday;
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public IDictionary<int, User> Referees { get; set; }
        public IDictionary<int, User> Spectators { get; set; }
        public IDictionary<int, User> Desks { get; set; }
        public IDictionary<int, IList<TeamShortDTO>> teamsByGroups { get; set; }
        public IDictionary <int, IList<AthleteShortDTO>> athletesByGroup { get; set; }
        public int Sort { get; set; }
        public int SeasonId { get; set; }
        public bool? IsPublished { get; set; }
		public string Section { get; set; }
        public static DateTime Today
        {
            get { return DateTime.Today; }
        }
        public static DateTime FirstDayOfMonth
        {
            get { return new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); }
        }
        public static DateTime Tomorrow
        {
            get
            {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
            }
        }

        public bool IsDepartmentLeague { get; internal set; }
        public int? DepartmentId { get; internal set; }
        public bool OnlyPublished { get; set; }
        public bool OnlyUnpublished { get; set; }
        public int? RoundStartCycle { get; set; }
    }
    public class Referees
    {
        public ICollection<User> RefereesItems { get; set; }
        public List<string> RefereeIds { get; set; }
        public string RefereesNames { get; set; }
        public int CycleId { get; set; }
        public int LeagueId { get; set; }
        public string ErrMessage { get; set; }
        public string ModalWindowId { get; set; }
        public string RefereeMainName { get; set; }
    }

    public class Desks
    {
        public ICollection<User> DesksItems { get; set; }
        public List<string> DesksIds { get; set; }
        public string DesksNames { get; set; }
        public int CycleId { get; set; }
        public int LeagueId { get; set; }
        public string ErrMessage { get; set; }
        public string DeskMainName { get; set; }
    }

    public class GameInLeague : GamesCycle
    {
        public GameInLeague(GamesCycle model)
        {
            CycleId = model.CycleId;
            StageId = model.StageId;
            CycleNum = model.CycleNum;
            StartDate = model.StartDate;
            AuditoriumId = model.AuditoriumId;
            GuestTeamId = model.GuestTeamId;
            GuestTeamPos = model.GuestTeamPos;
            GuestTeamScore = model.GuestTeamScore;
            GuestAthleteId = model.GuestAthleteId;
            HomeTeamId = model.HomeTeamId;
            HomeTeamPos = model.HomeTeamPos;
            HomeTeamScore = model.HomeTeamScore;
            HomeAthleteId = model.HomeAthleteId;
            RefereeIds = model.RefereeIds;
            SpectatorIds = model.SpectatorIds;
            DeskIds = model.DeskIds;
            GroupId = model.GroupId;
            GameStatus = model.GameStatus;
            TechnicalWinnnerId = model.TechnicalWinnnerId;
            MaxPlayoffPos = model.MaxPlayoffPos;
            MinPlayoffPos = model.MinPlayoffPos;
            BracketId = model.BracketId;
            Auditorium = model.Auditorium;
            Group = model.Group;
            GuestTeam = model.GuestTeam;
            HomeTeam = model.HomeTeam;
            TeamsPlayer = model.TeamsPlayer;
            TeamsPlayer1 = model.TeamsPlayer1;
            Stage = model.Stage;
            GameSets = model.GameSets;
            PlayoffBracket = model.PlayoffBracket;
            NotesGames = model.NotesGames;
            WallThreads = model.WallThreads;
            Users = model.Users;
            IsPublished = model.IsPublished;
            IsEilatTournament = model?.Group?.Stage?.League?.EilatTournament != null && model?.Group?.Stage?.League?.EilatTournament == true;
            RoundNum = model.RoundNum;
            TennisLeagueGames = model.TennisLeagueGames;
        }

        public int GameTypeId { get; internal set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public string Rank { get; set; }
        public bool IsEilatTournament { get; set; }
        public bool IsGameHasReligiousTeam { get; internal set; }
        public string DeskName { get; set; }
        public string DeskNames { get; set; }

        public string MainRefereeName { get; set; }
        public string RefereeNames { get; set; }

        
    }

    public class GamePartialModel : GameInLeague
    {
        public GamePartialModel(GameInLeague model): base(model)
        {
            GameTypeId = model.GameTypeId;
            LeagueId = model.LeagueId;
            LeagueName = model.LeagueName;
            Rank = model.Rank;
        }

        public int SeasonId { get; set; }
        public bool isWaitingDivision { get; set; }
        public bool HasTennisTechWinner { get; set; }
        public GameSet GoldenSet { get; set; }
        public GameSet Penalty { get; set; }
        public IDictionary<int, IList<TeamShortDTO>> teamsByGroups { get; set; }
        public IDictionary<int, IList<AthleteShortDTO>> athletesByGroups { get; set; }
        public int GameIndex { get; set; }
    }

    public class ScheduleStage
    {
        public int StageId { get; set; }
        public int StageNumber { get; set; }
        public string StageName { get; set; }
        public bool IsCrossesStage { get; set; }
        public IList<GameInLeague> Items { get; set; }
    }

    public class ScheduleGroup
    {
        public string GroupName { get; set; }
        public int BracketsCount { get; set; }
        public int GameTypeId { get; set; }
        public IList<ScheduleStage> Stages { get; set; }
        public bool IsIndividual { get; set; }
        public int? Rounds { get; set; }
        public int? RoundStartCycle { get; set; }
    }
}