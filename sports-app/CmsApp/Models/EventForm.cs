using AppModel;
using System;
using System.ComponentModel.DataAnnotations;
using CmsApp.Models.Mappers;

namespace CmsApp.Models
{
    public class EventForm
    {
        public EventForm()
        {
            EventTime = DateTime.Now.RoundUp(TimeSpan.FromMinutes(15));
        }

        public int EventId { get; set; }
        public int? LeagueId { get; set; }
        public string LeagueName { get; set; }
        public int? ClubId { get; set; }
        public string ClubName { get; set; }
        [Required] [StringLength(250)]
        public string Title { get; set; }
        [Required]
        public DateTime EventTime { get; set; }
        [Required] [StringLength(250)]
        public string Place { get; set; }        
        public DateTime CreateDate { get; set; }
        public bool IsPublished { get; set; }

        public string Description { get; set; }
        public int? UnionId { get; set; }
        public string UnionName { get; set; }
    }

    public class EventUpdateForm : Event
    {
        public bool RemoveEventImageFile { get; set; }
    }


}