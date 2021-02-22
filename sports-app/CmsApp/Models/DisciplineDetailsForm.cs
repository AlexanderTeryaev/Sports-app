using AppModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CmsApp.Models
{
    public class DisciplineDetailsForm
    {
        public int DisciplineId { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string PrimaryImage { get; set; }
        public string IndexImage { get; set; }
        [StringLength(250)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [StringLength(250)]
        [DataType(DataType.MultilineText)]
        public string IndexAbout { get; set; }
        public string Terms { get; set; }
        public int DocId { get; set; }
        public int? Class { get; set; }
        public string DisciplineType { get; set; }
        //for Gymnastics
        public List<RouteViewModel> Routes { get; set; }
        public List<RouteViewModel> TeamRoutes { get; set; }

        [MaxLength(255)]
        public string NewRote { get; set; }
        [Range(0, int.MaxValue)]
        public int? NewHierarchy { get; set; }

        [MaxLength(255)]
        public string NewTeamRote { get; set; }
        [Range(0, int.MaxValue)]
        public int? NewTeamHierarchy { get; set; }

        public int? UnionId { get; set; }
        public int? Format { get; set; }
        public int? SeasonId { get; set; }
        public InstrumentForm IntrumentForm { get; set; }
        public bool IsPreffered { get; set; }
        public bool RoadHeat { get; set; }
        public bool MountainHeat { get; set; }

        public int? NumberOfSportsmen { get; set; }
        public bool Coxwain { get; set; }
    }

    public class RouteViewModel
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Route { get; set; }
        public List<RankViewModel> Ranks { get; set; }
        public List<RankViewModel> TeamRanks { get; set; }
        [Range(0, int.MaxValue)]
        public int? Hierarchy { get; set; }

        public int RelationCount { get; set; }

        public bool HasError { get; set; }
    }

    public class RankViewModel
    {
        public RankViewModel(RouteRank model)
        {
            this.FromAge = model.FromAge;
            this.Id = model.Id;
            this.Rank = model.Rank;
            this.RelationCount = model.UsersRanks.Count;
            this.RouteId = model.RouteId;
            this.ToAge = model.ToAge;
            this.IsArchived = model.IsArchived;
        }

        public RankViewModel(RouteTeamRank model)
        {
            this.FromAge = model.FromAge;
            this.Id = model.Id;
            this.Rank = model.Rank;
            this.RelationCount = model.TeamsRanks.Count;
            this.RouteId = model.TeamRouteId;
            this.ToAge = model.ToAge;
        }

        public RouteRank GetModelUser()
        {
            return new RouteRank
            {
                FromAge = this.FromAge,
                Id = this.Id,
                Rank = this.Rank,
                RouteId = this.RouteId,
                ToAge = this.ToAge
            };
        }
        public RouteTeamRank GetModelTeam()
        {
            return new RouteTeamRank
            {
                FromAge = this.FromAge,
                Id = this.Id,
                Rank = this.Rank,
                TeamRouteId = this.RouteId,
                ToAge = this.ToAge
            };
        }

        public int Id { get; set; }
        public int RouteId { get; set; }
        public string Rank { get; set; }
        public DateTime? FromAge { get; set; }
        public DateTime? ToAge { get; set; }
        public bool? IsArchived { get; set; }

        public int RelationCount { get; set; }
    }
}