using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogLigFront.Models
{
    public class ExternalPlayerModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public int Gender { get; set; }
        public int? ClubId { get; set; }

        public string ClubName { get; set; }

        public int? TeamId { get; set; }

        public int? TeamName { get; set; }

        public string ProfileImage { get; set; }
        public string SectionAlias { get; set; }
    }
}