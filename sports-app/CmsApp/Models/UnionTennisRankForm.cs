using AppModel;
using DataService.DTO;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class UnionTennisRankForm
    {
        public int? CompetitionAgeId { get; set; }
        public IEnumerable<SelectListItem> ListAges { get; set; }
        public int? UnionId { get; set; }
        public int SeasonId { get; set; }
        public int? ClubId { get; set; }
        public int MinimumParticipationRequired { get; set; }
        public List<UnionTennisRankDto> RankList { get; set; }
    }
}