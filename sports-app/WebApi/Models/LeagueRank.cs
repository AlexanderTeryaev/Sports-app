using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class LeagueRank
    {
        public List<StageRank> Stages { get; set; }
    }

    public class StageRank
    {
        public string NameStage { get; set; }
        public List<GroupRank> Groups { get; set; }
    }

    public class GroupRank
    {
        public string NameGroup { get; set; }
        public List<TeamRank> Teams { get; set; }
    }

    public class TeamRank
    {
        public string Position { get; set; }
        public string Team { get; set; }
        public string Logo { get; set; }
    }
}