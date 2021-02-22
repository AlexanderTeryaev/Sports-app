
using DataService.DTO;
using System;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class TeamGalleryForm
    {
        public string url { get; set; }
        public DateTime Created { get; set; }
        public GalleryUserModel User { get; set; }
    }

    public class GalleryUserModel
    {
        public int Id { get; set; } // User id
        public String Name { get; set; } // User name
        public String Image { get; set; } // User image
    }
}