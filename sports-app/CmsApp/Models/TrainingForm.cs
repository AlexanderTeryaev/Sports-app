using AppModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CmsApp.Models.Mappers;

namespace CmsApp.Models
{
    public class TrainingForm
    {
        public TrainingForm()
        {
            TrainingTime = DateTime.Now.RoundUp(TimeSpan.FromMinutes(15));
        }

        public int SeasonId { get; set; }
        public int? ClubId { get; set; }

        public List<SelectListItem> TeamsSelectListItems { get; set; }
        public List<SelectListItem> AuditoriumSelectListItems { get; set; }
        
        [Required] [StringLength(250)]
        public string Title { get; set; }
        [Required]
        public DateTime TrainingTime { get; set; }

        public int? SelectedTeamId { get; set; }
        public int? AuditoriumId { get; set; }
        
        public string TrainingImage { get; set; }
        public string Content { get; set; }
        public bool IsTeam { get; set; }
    }
}