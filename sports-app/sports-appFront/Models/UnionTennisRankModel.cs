using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogLigFront.Models
{
    public class UnionTennisRankForm
    {
        public int? CompetitionAgeId { get; set; }
        public IEnumerable<SelectListItem> ListAges { get; set; }
        public int? UnionId { get; set; }
        public int SeasonId { get; set; }
        public int? ClubId { get; set; }

        public List<UnionTennisRankDto> RankList { get; set; }
    }
}