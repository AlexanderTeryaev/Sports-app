using System;
using System.Web;

namespace CmsApp.Models
{
    public class ImportAccountingBalanceViewModelBasic
    {
        public HttpPostedFileBase ImportFile { get; set; }

        public string ResultMessage { get; set; }
        public Guid ResultFileGuid { get; set; }

        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int DuplicateCount { get; set; }
        public string ExceptionMessage { get; set; }
    }

    public class ImportAccountingBalanceViewModel : ImportAccountingBalanceViewModelBasic
    {
        public ImportAccountingBalanceResult? Result { get; set; }
        public int? Id { get; set; }
        public int? UnionId { get; set; }
        public int? ClubId { get; set; }
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        public decimal? Income { get; set; }
        public decimal? Expense { get; set; }
        public string Comment { get; set; }
        public int? Reference { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public string FormName { get; set; }
    }

    public enum ImportAccountingBalanceResult
    {
        Success = 0,
        Error = 1,
        PartialyImported = 2,
    }
}