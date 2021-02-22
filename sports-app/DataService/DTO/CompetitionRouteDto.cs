using System;
using System.Collections.Generic;
using AppModel;

namespace DataService.DTO
{
    public class CompetitionRouteDto
    {
        public int DiciplineId { get; set; }
        public int SeasonId { get; set; }
        public int RouteId { get; set; }
        public int RankId { get; set; }
        public int LeagueId { get; set; }
        public int? Composition { get; set; }
        public string IntrumentsIds { get; set; }
        public int? SecondComposition { get; set; }
        public int? ThirdComposition { get; set; }
        public int? FourthComposition { get; set; }
        public int? FifthComposition { get; set; }
        public int? SixthComposition { get; set; }
        public int? SeventhComposition { get; set; }
        public int? EighthComposition { get; set; }
        public int? NinthComposition { get; set; }
        public int? TenthComposition { get; set; }
        public bool IsCompetitiveEnabled { get; set; }
    }

    public class CompetitionDto
    {
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        public string CompetitionName { get; set; }

        public int DisciplineId { get; set; }
        public string DisciplineName { get; set; }
        public List<WorkerMainShortDto> DisciplineReferees { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EndRegistrationDate { get; set; }
        public DateTime? StartTeamRegistrationDate { get; set; }
        public DateTime? EndTeamRegistrationDate { get; set; }
        public List<CompetitionRoute> CompetitionRoutes { get; internal set; }
        public bool IsEnded { get; set; }
        public bool IsStarted { get; set; }
        public bool IsMaxed { get; set; }
        public int? TypeId { get; internal set; }
        public DateTime? StartRegistrationDate { get; set; }
        public string Place { get; set; }
        public IEnumerable<CompetitionDisciplineDto> CompetitionDisciplines { get; set; }
        public List<CompetitionTeamRoute> CompetitionTeamRoutes { get; set; }
        public IEnumerable<WeightLiftingSession> Heats { get; set; }
        public int IsClubCompetition { get; set; }
        public bool NoTeamRegistration { get; set; }
        public string RegistrationLink { get; set; }
    }

    public class CompetitionAchievement
    {
        public string LeagueName { get; set; }
        public string Discipline { get; set; }
        public string StartDate { get; set; }
        public DateTime? EndDateDateTime { get; set; }
        public string EndDate { get; set; }
        public string Route { get; set; }
        public string Rank { get; set; }
        public string Instruments { get; set; }
        public string Composition { get; set; }
        public string Reserved { get; set; }
        public string FinalScore { get; set; }
        public string RankResult { get; set; }
        public string Position { get; set; }
        public string ClubName { get; internal set; }
        public int SeasonId { get; internal set; }
    }

    public class CompetitionAchievementBySeason
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public List<CompetitionAchievement> CompetitionAchievements { get; set; }
    }
}
