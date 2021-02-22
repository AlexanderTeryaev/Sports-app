using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class PlayerTeamApproveRetirementForm
    {
        public int SeasonId { get; set; }
        public int LeagueId { get; set; }
        public int ClubId { get; set; }
        public int TeamId { get; set; }

        public int ApproveTeamId { get; set; }

        [Required]
        public DateTime? ApproveDate { get; set; }

        [Required]
        public string ApproveText { get; set; }

        [Required]
        public int? ApproveAmount { get; set; }
    }
}