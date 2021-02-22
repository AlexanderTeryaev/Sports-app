using System.Collections.Generic;

namespace CmsApp.Models
{
    public class ActivitySelectTeamsDropdown
    {
        public string label { get; set; }
        public List<ActivitySelectTeamsDropdownItem> children { get; set; }
    }

    public class ActivitySelectTeamsDropdownItem
    {
        public string label { get; set; }
        public string value { get; set; }
        public bool selected { get; set; }
    }
}