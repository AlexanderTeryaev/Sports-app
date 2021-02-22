using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class DisciplineTabViewModel
    {
        public List<DisciplineViewModel> DisciplineViewModelsList { get; set; }
        public List<FriendshipTypeViewModel> FriendshipTypeViewModelsList { get; set; }
    }

    public class DisciplineViewModel
    {
        public int DisciplineId { get; set; }
        public string Name { get; set; }
        public int? Format { get; set; }
        public string DisciplineType { get; set; }
        public int? Class { get; set; }
        public int? NumberOfSportsmen { get; set; }
        public bool RoadHeat { get; set; }
        public bool MountainHeat { get; set; }
        public bool Coxwain { get; set; }
    }

    public class FriendshipTypeViewModel
    {
        public int FriendshipsTypesId { get; set; }
        public int UnionId { get; set; }
        public string Name { get; set; }
        public int SeasonId { get; set; }
        public int? Hierarchy { get; set; }
    }
}