using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using AppModel;
using WebApi.Services;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class BannerViewModel
    {
        public int BannerId { get; set; }
        
        public string LinkUrl { get; set; }

        public string ImageName { get; set; }
    }
}