using System;
using System.ComponentModel.DataAnnotations;

namespace CmsApp.Models
{
    public class DaysForHostingViewModel
    {
        public int? Id { get; set; }
        [Required]
        public DayOfWeek Day { get; set; }
        [Required]
        public string StartTime { get; set; }
        [Required]
        public string EndTime { get; set; }
    }
}