using System;

namespace DataService.DTO
{
    public class DaysForHostingDto
    {
        public int? Id { get; set; }
        public DayOfWeek Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}