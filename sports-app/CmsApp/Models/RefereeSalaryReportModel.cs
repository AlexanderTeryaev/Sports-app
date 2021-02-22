using System;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class RefereeSalaryReportModel
    {
        public bool CanRemoveReports { get; set; }
        public List<RefereeSalaryReportItemModel> Reports { get; set; }
    }

    public class RefereeSalaryReportItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SeasonName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}