using System;
using DataService.DTO;

namespace CmsApp.Models
{
    public class SalaryReportForm
    {
        public string OfficialType { get; set; }
        public DateTime ReportStartDate { get; set; }
        public DateTime ReportEndDate { get; set; }
        public WorkerReportDTO WorkerReportInfo { get; internal set; }
        public int JobId { get; set; }
    }
}