using System;

namespace DataService.DTO
{
    public class TennisGameCycleDto
    {
        public int GameId { get; set; }
        public int? HomePlayerId { get; set; }
        public int? GuestPlayerId { get; set; }
        public int? HomePairPlayerId { get; set; }
        public int? GuestPairPlayerId { get; set; }
    }

    public class TennisGameCycleCompetitionDto
    {
        public int AuditoriumId { get; set; }
        public string Auditorium { get; set; }
        public string AuditoriumAddress { get; set; }
        public int CycleId { get; set; }
        public int GameType { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryLogo { get; set; }
        public string GameStatus { get; set; }
        public int? FirstPlayerId { get; set; }
        public string FirstPlayerImage { get; set; }
        public bool IsFirstPlayerKnown { get; set; }
        public int FirstPlayerScore { get; set; }
        public string SecondPlayerImage { get; set; }     
        public int? SecondPlayerId { get; set; }
        public bool IsSecondPlayerKnown { get; set; }
        public int SecondPlayerScore { get; set; }
        public int StageId { get; set; }
        public string StageName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime StartDate { get; set; }
        public int? MaxPlayoffPos { get; set; }
        public int? MinPlayoffPos { get; set; }
        public bool IsPublished { get; set; }
        public int? WinnerId { get; set; }
        public string PdfGameReport { get; set; }
        public int BracketIndex { get; set; }
        public string FirstPlayer { get; internal set; }
        public string SecondPlayer { get; internal set; }
        public BracketDto Bracket { get; internal set; }
        public bool IsAdvanced { get; internal set; }
        public object IndexInBracket { get; internal set; }
        public bool IsRoot { get; internal set; }
        public int? RoundNum { get; set; }
        public int? CycleNum { get; set; }
        public bool IsDivision { get; internal set; }
        public bool IsNotSetYet { get; internal set; }
        public string TimeInitial { get; internal set; }
    }
}
