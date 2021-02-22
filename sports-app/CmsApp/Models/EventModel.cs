using AppModel;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class EventModel
    {
        public int? LeagueId { get; set; }
        public int? ClubId { get; set; }
        public int? UnionId { get; set; }
        public List<Event> EventList { get; set; }
        public bool isCollapsable { get; set; } = true;
    }
}