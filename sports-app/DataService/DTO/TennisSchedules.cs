using AppModel;
using System;
using System.Collections.Generic;

namespace DataService.DTO
{
    public class TennisSchedules
    {
        public class DateFilterPeriod
        {
            public const int BeginningOfMonth = 0;
            public const int Ranged = 1;
            public const int All = 2;
        }
        public int CategoryId { get; set; }
        public int? UnionId { get; set; }
        public IList<TennisScheduleGroup> Groups { get; set; }
        public IList<TennisGameCycle> Games { get; set; }
        public List<AuditoriumShort> Auditoriums { get; set; }
        public int dateFilterType { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public IDictionary<int, User> Referees { get; set; }
        public IDictionary<int, User> Spectators { get; set; }
        public IDictionary<int, IList<TeamShortDTO>> teamsByGroups { get; set; }
        public IDictionary<int, IList<AthleteShortDTO>> athletesByGroup { get; set; }
        public int Sort { get; set; }
        public int SeasonId { get; set; }
        public bool? IsPublished { get; set; }
        public string Section { get; set; }
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

        public bool IsDepartmentLeague { get; set; }
        public int? DepartmentId { get; set; }
        public bool OnlyPublished { get; set; }
        public bool OnlyUnpublished { get; set; }
        public int? RoundStartCycle { get; set; }
    }

    public class TennisGameInCategory : TennisGameCycle
    {
        public TennisGameInCategory(TennisGameCycle model)
        {
            CycleId = model.CycleId;
            StageId = model.StageId;
            CycleNum = model.CycleNum;
            StartDate = model.StartDate;
            FieldId = model.FieldId;
            FirstPlayerId = model.FirstPlayerId;
            SecondPlayerId = model.SecondPlayerId;
            SecondPlayerScore = model.SecondPlayerScore;
            FirstPlayerScore = model.FirstPlayerScore;
            RefereeIds = model.RefereeIds;
            SpectatorIds = model.SpectatorIds;
            GroupId = model.GroupId;
            GameStatus = model.GameStatus;
            MaxPlayoffPos = model.MaxPlayoffPos;
            MinPlayoffPos = model.MinPlayoffPos;
            BracketId = model.BracketId;
            Auditorium = model.Auditorium;
            TennisGroup = model.TennisGroup;
            TeamsPlayer = model.TeamsPlayer;
            TeamsPlayer1 = model.TeamsPlayer1;
            //TennisStage = model.TennisStage;
            TennisGameSets = model.TennisGameSets;
            TennisPlayoffBracket = model.TennisPlayoffBracket;
            IsPublished = model.IsPublished;
            FirstPlayerPairId = model.FirstPlayerPairId;
            SecondPlayerPairId = model.SecondPlayerPairId;
            FirstPairPlayer = model.FirstPairPlayer;
            SecondPairPlayer = model.SecondPairPlayer;
            RoundNum = model.RoundNum;
            TimeInitial = model.TimeInitial;
            TennisGameCycle = model;
        }
        public TennisGameCycle TennisGameCycle { get; set; }
        public int GameTypeId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Rank { get; set; }
    }

    public class TennisGamePartialModel : TennisGameCycle
    {
        public TennisGamePartialModel(TennisGameCycle model)
        {
            GameTypeId = model.GameTypeId;
            CategoryId = model.CategoryId;
            CategoryName = model.CategoryName;
            Rank = model.Rank;
        }

        public int SeasonId { get; set; }
        public bool isWaitingDivision { get; set; }
        public GameSet GoldenSet { get; set; }
        public GameSet Penalty { get; set; }
        public IDictionary<int, IList<TeamShortDTO>> teamsByGroups { get; set; }
        public IDictionary<int, IList<AthleteShortDTO>> athletesByGroups { get; set; }
    }

    public class TennisScheduleStage
    {
        public int StageId { get; set; }
        public int StageNumber { get; set; }
        public IList<TennisGameCycle> Items { get; set; }
    }

    public class TennisScheduleGroup
    {
        public string GroupName { get; set; }
        public int BracketsCount { get; set; }
        public int GameTypeId { get; set; }
        public IList<TennisScheduleStage> Stages { get; set; }
        public bool IsIndividual { get; set; }
        public int? Rounds { get; set; }
    }
}