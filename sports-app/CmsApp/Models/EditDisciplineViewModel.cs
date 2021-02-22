using System.Collections.Generic;
using AppModel;

namespace CmsApp.Models
{
    public class EditDisciplineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UnionName { get; set; }
        public int? UnionId { get; set; }
        public int? SeasonId { get; set; }
        public IEnumerable<Season> Seasons { get; set; }
        public string SectionAlias { get; set; }
    }
}