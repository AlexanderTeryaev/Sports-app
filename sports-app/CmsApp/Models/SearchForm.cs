using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class SearchForm
    {
        public string Search { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Title { get; set; }
        public int? CatId { get; set; }
        public IEnumerable<SelectListItem> CatsList { get; set; }
        public IEnumerable<SelectListItem> TypesList { get; set; }
        public IEnumerable<SelectListItem> StatsList { get; set; }
        public int? Status { get; set; }
        public int? TypeId { get; set; }
        public int FullSearch { get; set; }
    }
}