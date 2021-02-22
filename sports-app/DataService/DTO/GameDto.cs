using System;
using System.Collections.Generic;
using AppModel;

namespace DataService.DTO
{
    public class BaseGameDto
    {
        public int GameId { get; set; }
        public string GameCycleStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? HomeTeamId { get; set; }
        public string HomeTeamTitle { get; set; }
        public int HomeTeamScore { get; set; }
        public string HomeTeamLogo { get; set; }
        public int? GuestTeamId { get; set; }
        public string GuestTeamTitle { get; set; }
        public int GuestTeamScore { get; set; }
        public string GuestTeamLogo { get; set; }
        public string Auditorium { get; set; }
        public int CycleNumber { get; set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public decimal MaxHandicap { get; set; }
        public bool IsHandicapEnabled { get; internal set; }


    }
    public class GameDto : BaseGameDto
    {
        public string AuditoriumAddress { get; set; }
        public bool IsPublished { get; set; }
        public int DisciplineId { get; set; }
        public string DisciplineName { get; set; }
        public int StageId { get; set; }
        public IEnumerable<TeamDetailsDto> HomeTeamDetails { get; set; }
        public IEnumerable<TeamDetailsDto> GuestTeamDetails { get; set; }
        public TeamDetailsDto HomeTeamDetail { get; set; }
        public TeamDetailsDto GuestTeamDetail { get; set; }
        public int? SeasonId { get; set; }
        public int? UnionId { get; set; }
        public string HomeTeamTitlePair { get; set; }
        public string GuestTeamTitlePair { get; set; }
        //for tennis 
        public bool isFromCategoryDates { get; set; }
        public int? ClubTeamId { get; set; }
    }
}
