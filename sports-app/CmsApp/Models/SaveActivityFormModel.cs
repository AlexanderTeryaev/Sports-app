using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsApp.Helpers;
using DataService.Utils;

namespace CmsApp.Models
{
    public class SaveActivityFormModel
    {
        public string FormName { get; set; }
        public string FormDescription { get; set; }

        public List<ActivityFormFieldItem> Fields { get; set; }
    }

    public class ActivityFormFieldItem
    {
        public ActivityFormControlType Type { get; set; }
        public string PropertyName { get; set; }
        public string LabelTextEn { get; set; }
        public string LabelTextHeb { get; set; }
        public string LabelTextUk { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanBeRequired { get; set; }
        public bool CanBeDisabled { get; set; }
        public bool CanBeRemoved{ get; set; }
        public bool HasOptions{ get; set; }
        public string FieldNote { get; set; }
        public string CustomDropdownValues { get; set; } //JSON
        public int? CustomPriceId { get; set; }
    }
}