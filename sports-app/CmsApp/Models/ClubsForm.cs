using AppModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataService.DTO;

namespace CmsApp.Models
{
    public class ClubsForm
    {
        public bool isEditorOrUnionManager { get; set; } = false;
        public int? ClubId { get; set; }
        public int? SectionId { get; set; }
        public int? UnionId { get; set; }
        public int? SeasonId { get; set; }
        [Required]
        public string Name { get; set; }
        public List<Club> Clubs { get; set; }
        public List<ActivityFormsSubmittedData> ClubsRegistrations { get; set; }
        public Dictionary<int, ClubsPlayersInfoDto> ClubPlayersInforamtion { get; set; }
        public bool IsIndividualSection { get; set; }
        public string SectionAlias { get; set; }
        public bool IsFlowerOfSport { get; set; }

        public bool IsGymnasticUnion { get; set; }
        public bool CanApproveClubs { get; set; }
        public bool IsRegionalLevelEnabled { get; set; }
    }

    public class RegionalClubsForm
    {
        public bool IsRegionalFederation { get; set; }
        public int? RegionalId { get; set; }
        public int? SectionId { get; set; }
        public int? UnionId { get; set; }
        public int? SeasonId { get; set; }
        public IEnumerable<string> SelectedValues { get; set; }        
      //  public IEnumerable<Club> Clubs { get; set; }
        public IEnumerable<RegionalClub> RegionalClubs { get; set; }

        public System.Web.Mvc.MultiSelectList DdlOptions { get; set; }

    }

    public class RegionalClub
    {       
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public string ClubManager { get; set; }
        public bool IsClubApproveByRegional { get; set; }
    }
}