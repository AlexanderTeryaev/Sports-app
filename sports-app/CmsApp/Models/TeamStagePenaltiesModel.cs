using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppModel;
using DataService.LeagueRank;

namespace CmsApp.Models
{
    public class TeamStagePenaltiesModel
    {
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }

        public List<TeamPenalty> TeamPenalties { get; set; }
        public RankStage Stage { get; set; }
        public List<RankGroup> Groups { get; set; }

        public bool CanEditPenalties { get; set; }
    }
}