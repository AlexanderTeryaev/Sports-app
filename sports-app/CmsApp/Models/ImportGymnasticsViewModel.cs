using System;
using System.Web;

namespace CmsApp.Models
{
    public class ImportViewModelBasic
    {
        public HttpPostedFileBase ImportFile { get; set; }

        public ImportPlayersResult? Result { get; set; }
        public string ResultMessage { get; set; }
        public Guid ResultFileGuid { get; set; }

        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int DuplicateCount { get; set; }
        public string ExceptionMessage { get; set; }
    }

    public class ImportSportsmanViewModel : ImportViewModelBasic
    {
        public int? ClubId { get; set; }
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        public int? CompetitionRouteId { get; set; }
        public string FormName { get; set; }
    }
}