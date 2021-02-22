

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class NotificationsForm
    {
        public int? SeasonId { get; set; }
        public int EntityId { get; set; }

        public LogicaName RelevantEntityLogicalName { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public bool NeedHideTextField { get; set; }
        public bool SendByEmail { get; set; }
        public string Subject { get; set; }
        public IEnumerable<SelectListItem> ClubManagers { get; set; }
        public IEnumerable<SelectListItem> TeamManagers { get; set; }
        public IEnumerable<int> ClubManagerIds { get; set; }
        public IEnumerable<int> TeamManagerIds { get; set; }
        public bool IsClubEmailChecked { get; set; }
        public bool IsUnionManager { get; set; }
        public string ClubEmail { get; set; }
        public string UnionEmail { get; set; }
        public int? ClubId { get; set; }
        public int? LeagueId { get; set; }
        public int? UnionId { get; set; }
        public int TeamId { get; set; }
    }

    public class NotificationsMultipleForm
    {
        public int? SeasonId { get; set; }
        public int[] PlayersIds { get; set; }

        [Required]
        [MaxLength(500)]
        public string Subject { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public bool NeedHideTextField { get; set; }
        public bool SendByEmail { get; set; }
        public int ActivityId { get; set; }
        public string UnionEmail { get; set; }
        public string ClubEmail { get; set; }
        public bool IsUnionManager { get; set; }
        public bool IsClubEmailChecked { get; set; }
    }
}