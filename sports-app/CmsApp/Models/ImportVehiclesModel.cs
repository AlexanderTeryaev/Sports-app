using System;
using System.Web;

namespace CmsApp.Models
{
    public class ImportVehiclesViewModel
    {
        public int UnionId { get; set; }
        public int SeasonId { get; set; }

        public HttpPostedFileBase ImportFile { get; set; }

        public ImportPlayersResult? Result { get; set; }
        public string ResultMessage { get; set; }
        public Guid ResultFileGuid { get; set; }

        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int DuplicateCount { get; set; }

        public string FormName { get; set; }
        public bool IsException { get; internal set; }
        public string ExceptionMessage { get; internal set; }
    }
}