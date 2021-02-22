using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityFormEditModel
    {
        public string FormName { get; set; }
        public string FormDescription { get; set; }
        public DateTime? EndDate { get; set; }
        public string FormContent { get; set; }
    }
}