using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class DepartmentsForm
    {
        public int ParentClubId { get; set; }
        public int? SeasonId { get; set; }
        public ICollection<DeparmentModel> Deparments { get; set; }
        public IEnumerable<SelectListItem> Sports { get; set; }
    }

    public class DeparmentModel
    {
        public int? DepartmentId { get; set; }
        public string DepartmentTitle { get; set; }
        public int? SelectedSportId { get; set; }
        public IEnumerable<SelectListItem> Sports { get; set; }
        public int? ParentSectionId { get; set; }
    }
}