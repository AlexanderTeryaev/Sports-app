using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class MediaForm
    {
        public int FileId { get; set; }
        public int? GalleryId { get; set; }
        public int FileType { get; set; }
        public int TypeId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
        public string MediaUrl { get; set; }
        public string Referrer { get; set; }
        public HttpPostedFileBase ImgFile { get; set; }
        public IEnumerable<SelectListItem> MediaTypes { get; set; }
    }
}