using System;

namespace CmsApp.Models
{
    public class PlayerHistoryFormView
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public string Player { get; set; }
        public string Team { get; set; }
        public string OldTeam { get; set; }
        public DateTime Date { get; set; }
        public string UserActionName { get; internal set; }
    }
}