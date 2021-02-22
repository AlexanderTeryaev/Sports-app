using System;

namespace CmsApp.Models
{
    public class CompetitionAgeViewModel
    {
        public string AgeName { get; set; }
        public DateTime? FromBirth { get; set; }
        public DateTime? ToBirth { get; set; }
        public string Gender { get; set; }
    }
}