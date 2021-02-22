using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityBranchViewModel {
        public int ActivityBranchId { get; set; }
        public string ActivityBranchName { get; set; }
        public bool Collapsed { get; set; }

        public List<ActivityViewModel> Activities { get; set; }
    }

    public class ActivityViewModel
    {
        public int ActivityId { get; set; }
        public bool IsPublished { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string SeasonName { get; set; }

        public int ActivityBranchId { get; set; }
        public string ActivityBranchName { get; set; }

        public bool IsReadOnly { get; set; }

        public bool ByBenefactor { get; set; }
        public bool CanAttachDocument { get; set; }
        public bool CanAttachMedicalCert { get; set; }
        public bool CanAttachInsuranceCert { get; set; }
        public bool IsAutomatic { get; set; }

        public int RegistationsCount { get; set; }
        public int InactiveRegistrationsCount { get; set; }
    }
}