using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{

    public class ActivitiesViewModel
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        private string ExternalLink { get; set; }
    }
}