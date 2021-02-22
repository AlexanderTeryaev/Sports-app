using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class GalleryForm
    {
        public int GalleryId { get; set; }
        public int? IconId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
        public DateTime? Modified { get; set; }
        public string UserName { get; set; }
        public HttpPostedFileBase ImgFile { get; set; }
        public string Referrer { get; set; }
        public SelectList IconsList { get; set; }
    }
}