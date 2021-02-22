using System;
using System.Collections.Generic;
using DataService.DTO;

namespace DataService
{
    public class GamesCycleDto
    {
        public int CycleId { get; set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public string LeagueLogo { get; set; }
        public string SectionAlias { get; set; }
        public DateTime StartDate { get; set; }
        public string GameStatus { get; set; }
        public int AuditoriumId { get; set; }
        public string Auditorium { get; set; }
        public string AuditoriumAddress { get; set; }
        public string HomeTeam { get; set; }
        public string GuestTeam { get; set; }
        public string HomeLogo { get; set; }
        public string GuesLogo { get; set; }
        public int? HomeTeamId { get; set; }
        public int? GuestTeamId { get; set; }
        public int? GuestAthleteId { get; set; }
        public int? HomeAthleteId { get; set; }
        public int HomeTeamScore { get; set; }
        public int GuestTeamScore { get; set; }
        public BracketDto Bracket { get; set; }
        public int BracketIndex { get; set; }
        public EventDto EventRef { get; set; }
        public int StageId { get; set; }
        public string StageName { get; internal set; }
        public string StageCustomName { get; set; }
        public bool IsCrossesStage { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public bool IsPublished { get; set; }
        public bool IsAdvanced { get; internal set; }
        public bool IsRoot { get; internal set; }
        public bool IsHomeTeamKnown { get; internal set; }
        public bool IsGuestTeamKnown { get; internal set; }
        public int IndexInBracket { get; internal set; }
        public int? MaxPlayoffPos { get; internal set; }
        public int? MinPlayoffPos { get; internal set; }
        public int GameType { get; set; }
        public int BasketBallWaterpoloHomeTeamScore { get; set; }
        public int BasketBallWaterpoloGuestTeamScore { get; set; }
        public int? PenaltyHomeTeamScore { get; set; }
        public int? PenaltyGuestTeamScore { get; set; }
        public string Rank { get; internal set; }
        public int? WinnerId { get; internal set; }
        public bool IsTennisLeagueGame { get; set; }
        public string PdfGameReport { get; set; }
        public int? RoundNum { get; set; }
        public int? CycleNum { get; set; }
        public bool IsDivision { get; set; }
        public bool HasHomeTennisTechWinner { get; set; }
        public bool HasGuestTennisTechWinner { get; set; }
        public int CrossesRank { get; set; }
        public bool IsNotSetYet { get; internal set; }
        public string Remark { get; set; }
    }

    public class BracketDto
    {
        public int Id { get; internal set; }
        public int Type { get; internal set; }
    }

    public class EventDto
    {
        public EventDto()
        {
            IsUsed = false;
        }
        public int EventId { get; set; }
        public int? LeagueId { get; set; }
        public int? ClubId { get; set; }
        public DateTime EventTime { get; set; }

        public string Title { get; set; }
        public string Place { get; set; }
        public bool IsUsed { get; set; }
    }

    public class SchedulesDto
    {
        public string gameAlias { get; set; }
        public IEnumerable<GamesCycleDto> GameCycles { get; set; }
        public IEnumerable<GroupBracketsDto> BracketData { get; set; }
        public IEnumerable<EventDto> Events { get; set; }
        public bool NeedShowCycles { get; set; }
        public int? RoundStartCycle { get; set; }

    }

    public class CompetitionSchedulesDto
    {
        public IEnumerable<TennisGameCycleCompetitionDto> GameCycles { get; set; }
        public IEnumerable<CompetitionGroupBracketsDto> BracketData { get; set; }
        public IEnumerable<EventDto> Events { get; set; }
        public bool NeedShowCycles { get; set; }
        public int? RoundStartCycle { get; set; }
    }



    public class StageBracketsDto
    {
        public string elementWinnerId { get; set; }
        public string elementLoserId { get; set; }
        public int rounds { get; set; }
        public IList<StageCyclesDto> stages { get; set; }
    }

    public class CompetitionStageBracketsDto
    {
        public string elementWinnerId { get; set; }
        public string elementLoserId { get; set; }
        public int rounds { get; set; }
        public IList<CompetitionStageCyclesDto> stages { get; set; }
    }

    public class StageCyclesDto
    {
        public string StageName { get; set; }
        public string StageCustomName { get; set; }
        public IList<GamesCycleDto> Items { get; set; }
    }

    public class CompetitionStageCyclesDto
    {
        public string StageName { get; set; }
        public IList<TennisGameCycleCompetitionDto> Items { get; set; }
    }

    public class GroupBracketsDto
    {
        public string GroupName { get; set; }
        public int GameTypeId { get; set; }
        public IList<StageCyclesDto> Stages { get; set; }
    }

    public class CompetitionGroupBracketsDto
    {
        public string GroupName { get; set; }
        public int GameTypeId { get; set; }
        public IList<CompetitionStageCyclesDto> Stages { get; set; }
    }
}
