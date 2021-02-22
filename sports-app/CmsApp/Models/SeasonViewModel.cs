using System.Collections.Generic;
using AppModel;

namespace CmsApp.Models
{
    public class SeasonViewModel
    {
        public int EntityId { get; set; }
        public LogicaName LogicalName { get; set; }
        public int? SeasonId { get; set; }
        public IEnumerable<Season> Seasons { get; set; }
    }
}