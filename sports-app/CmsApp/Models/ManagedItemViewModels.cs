using System;
using DataService.DTO;
using System.Collections.Generic;
using AppModel;

namespace CmsApp.Models
{
    public class ManagedItemViewModel
    {
        public ManagedItemViewModel()
        {
            this.Unblockaded = new List<PlayersBlockadeShortDTO>(); 
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string LeagueName { get; set; }

        public string Controller { get; set; }

        public int? SeasonId { get; set; }

        public int? LeagueId { get; set; }
        public string LeagueStartDate { get; set; }
        public string LeagueEndDate { get; set; }

        public int? UnionId { get; set; }

        public int? ClubId { get; set; }

        public IList<PlayersBlockadeShortDTO> Unblockaded { get; set; }
        public string SeasonName { get; set; }
        public string UnionName { get; set; }
        public string ClubName { get; set; }
        public string JobName { get; set; }
        public string JobRole { get; set; }
        public bool IsMultiple { get; set; }
        public Section Section { get; set; }
        public string SectionName { get; set; }
        public string Url { get; set; }
        public string TeamName { get; internal set; }
        public int? TeamId { get; set; }
        public string KarateClubMessage { get; set; }
        public string RegionName { get; set; }
        public DateTime? SeasonEndDate { get; set; }
    }
}