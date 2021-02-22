using System.Collections.Generic;

namespace WebApi.Models
{
    public class SectionViewModel
    {
        public int SectionId { get; set; }
        public int LangId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsIndividual { get; set; }

        public List<ClubTeamInfoViewModel> Clubs { get; set; }

    }
    public class ClubViewModel
    {
        public int clubId { get; set; }
        public string Name { get; set; }

    }

}