using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class SportRankViewModel
    {
        public int Id { get; set; }
        public int SportId { get; set; }
        public string RankName { get; set; }
        public string RankNameHeb { get; set; }
    }
}