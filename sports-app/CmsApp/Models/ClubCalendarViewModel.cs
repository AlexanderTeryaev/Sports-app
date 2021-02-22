using System.Collections.Generic;

namespace CmsApp.Models
{
    public class ClubCalendarViewModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string Color { get; set; }
        public string auditorium { get; set; }
        public bool hasImage { get; set; }
        public List<string> attendanceList { get; set; }
        public string teamName { get; set; }
        public string color { get; set; }
    }
}