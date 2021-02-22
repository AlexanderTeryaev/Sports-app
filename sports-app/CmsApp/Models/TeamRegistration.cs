using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppModel;

namespace CmsApp.Models
{
    public class TeamRegistrationModel
    {
        public bool IsActive { get; set; }
        public int LeagueId { get; set; }
        public string ActivityName { get; set; }
        public bool IsAutomatic { get; set; }
        public List<ActivityFormsDetail> FormControls { get; set; }
        public List<ActivityFormCustomField> CustomFields { get; set; }
        public CultEnum Culture { get; set; }
    }
}