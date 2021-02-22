using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class PlayerTeamRetirementForm
    {
        public int SeasonId { get; set; }
        public int LeagueId { get; set; }
        public int ClubId { get; set; }
        public int TeamId { get; set; }

        public int RetirementTeamId { get; set; }

        [Required]
        [MinLength(1)]
        public string Reason { get; set; }
    }
}