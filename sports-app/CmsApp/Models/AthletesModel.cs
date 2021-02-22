using AppModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class AthletesModel
    {
        public AthletesModel()
        {
            WeightSelector = new List<SelectListItem>();
        }
        public int? SeasonId { get; set; }
        public int LeagueId { get; set; }
        public int StageId { get; set; }
        public int[] SelectedAthletesIds { get; set; }
        public int[] SelectedPlayoffAthletesIds { get; set; }
        public bool IsAgesEnabled { get; set; }
        public bool IsRankedEnabled { get; set; }
        public bool IsWeightEnabled { get; set; }
        public DateTime? AgeStart { get; set; }
        public DateTime? AgeEnd { get; set; }
        public string WeightType { get; set; }
        public List<int> SelectedRanks { get; set; }
        public List<SelectListItem> WeightSelector { get; set; }
        public List<SportRank> Ranks { get; set; }
        public int? WeightFrom { get; set; }
        public int? WeightTo { get; set; }
        public int? TennisRank { get; set; }
        public int? TennisRankPoints { get; set; }
    }
}