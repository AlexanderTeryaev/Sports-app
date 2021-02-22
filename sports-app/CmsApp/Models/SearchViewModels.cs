using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class SearchViewModel
    {
        public bool IsSearchLeagueVisible { get; set; }
        public bool IsSearchTeamVisible { get; set; }
        public bool IsSearchPlayerVisible { get; set; }
    }
}