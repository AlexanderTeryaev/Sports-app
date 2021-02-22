using AppModel;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class BenefitModel
    {
        public int SeasonId { get; set; }
        public int UnionId { get; set; }
        public List<Benefit> BenefitList { get; set; }
        public bool isCollapsable { get; set; } = true;
    }
}