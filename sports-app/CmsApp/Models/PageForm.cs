using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CmsApp.Models
{
    public class PageForm
    {
        public int PageId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public int? PgTypeId { get; set; }
        public string FileName { get; set; }
        public DateTime? Modified { get; set; }
        public string UserName { get; set; }
        public HttpPostedFileBase ImgFile { get; set; }
        public string Referrer { get; set; }
        public IEnumerable<SelectListItem> PageTypes { get; set; }
    }
}