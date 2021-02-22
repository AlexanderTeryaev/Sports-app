using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using AppModel;

namespace CmsApp.Models
{
    public class JobForm
    {
        public int JobId { get; set; }
        public int SectionId { get; set; }
        public int? RoleId { get; set; }
        [Required]
        public string JobName { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }
        public IEnumerable<Job> JobsList { get; set; }
    }
}