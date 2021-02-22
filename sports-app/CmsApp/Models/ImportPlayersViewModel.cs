using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ImportPlayersViewModel
    {
        public int? TeamId { get; set; }
        public int? LeagueId { get; set; }
        public int? UnionId { get; set; }
        public int? ClubId { get; set; }
        public int? SeasonId { get; set; }
        public string FormName { get; set; }
        public HttpPostedFileBase ImportFile { get; set; }

        public bool CanApprovePlayers { get; set; }
        // sets both approved and active 
        public bool ApprovePlayersOnImport { get; set; }
        // set only active
        public bool SetPlayersOnlyAsActiveOnImport { get; set; }

        public ImportPlayersResult? Result { get; set; }
        public string ResultMessage { get; set; }
        public Guid ResultFileGuid { get; set; }

        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int DuplicateCount { get; set; }
        public bool IsTennis { get; internal set; }
        public bool IsSectionClub { get; internal set; }

        public string UpdateTargetId { get; set; }
    }


    public enum ImportPlayersResult
    {
        Success = 0,
        Error = 1,
        PartialyImported = 2,
    }
}