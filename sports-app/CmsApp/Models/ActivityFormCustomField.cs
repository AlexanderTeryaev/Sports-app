using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CmsApp.Helpers;

namespace CmsApp.Models
{
    public class ActivityFormCustomField
    {
        public string PropertyName { get; set; }
        public ActivityFormControlType Type { get; set; }
        public string Value { get; set; }
    }
}