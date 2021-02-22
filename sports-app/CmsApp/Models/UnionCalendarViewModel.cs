namespace CmsApp.Models
{
    public class UnionCalendarViewModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public bool allDay { get; set; }
        public string color { get; set; }
        public string typeOfEvent { get; set; }
        public string auditorium { get; set; }
        public string eventImage { get; set; }
        public string leagueName { get; set; }
        public string result { get; set; }
        public int? leagueId { get; set; }
        public string teamsNames;
    }

    /// <summary>
    /// Used for union calendar view
    /// </summary>
    public static class CalendarEventTypes
    {
        public const string EVENT = "EVENT";
        public const string COMPETITION = "COMPETITION";
        public const string GAME = "GAME";
        public const string NEWGAME = "NEWGAME";
    }
}