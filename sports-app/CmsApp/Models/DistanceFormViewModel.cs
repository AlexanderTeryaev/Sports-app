using System.Collections.Generic;

namespace CmsApp.Models
{
    public class DistanceTableForm
    {
        public int RelevantId { get; set; }
        public LogicaName RelevantLogicalName { get; set; }
        public IEnumerable<string> Cities { get; set; }
        public int? SeasonId { get; set; }
    }
}