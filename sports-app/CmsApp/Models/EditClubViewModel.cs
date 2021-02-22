using System.Collections.Generic;
using AppModel;

namespace CmsApp.Models
{
    public class EditClubViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? SectionId { get; set; }
        public string SectionName { get; set; }
        public int? UnionId { get; set; }
        public int? ClubId { get; set; }
        public string UnionName { get; set; }
        public int? SeasonId { get; set; }
        public int CurrentSeasonId { get; set; }
        public string CurrentSeasonName { get; set; }
        public IEnumerable<Season> Seasons { get; set; }
        public bool IsClubTrainingEnabled { get; set; }
        public bool SectionIsIndividual { get; internal set; }
        public bool? IsUnionClub { get; set; }
        public int? ParentClubId { get; set; }
        public string ParentClubTitle { get; set; }
        public int? ParentClubSectionId { get; set; }
        public int? SportId { get; set; }
        public bool CanEditClub { get; set; }
        private bool isCatchBall { get; set; }
        public bool IsCatchBall {
            get
            {
                return isCatchBall || SectionName == GamesAlias.NetBall;
            }
            set
            {
                isCatchBall = value;
            }
        }
        private bool isTennis { get; set; }
        public bool IsTennis
        {
            get
            {
                return isTennis || SectionName == GamesAlias.Tennis;
            }
            set
            {
                isTennis = value;
            }
        }

        private bool isGymnastics { get; set; }
        public bool IsGymnastics
        {
            get
            {
                return isGymnastics || SectionName == GamesAlias.Gymnastic;
            }
            set
            {
                isGymnastics = value;
            }
        }

        private bool isMartialArts { get; set; }
        public bool IsMartialArts
        {
            get
            {
                return isMartialArts || SectionName == GamesAlias.MartialArts;
            }
            set
            {
                isMartialArts = value;
            }
        }


        public bool IsUnionClubManagerUnderPastSeason { get; set; }
        public bool IsWeightLifting => SectionName == GamesAlias.WeightLifting;
        public bool IsAthletics => SectionName == GamesAlias.Athletics;
        public bool IsWaveSurfing => SectionName == GamesAlias.WaveSurfing;
        public bool IsSwimming => SectionName == GamesAlias.Swimming;
        public bool IsRowing => SectionName == GamesAlias.Rowing;
        public bool IsBicycle => SectionName == GamesAlias.Bicycle;
        public bool IsWaterPolo => SectionName == GamesAlias.WaterPolo;
        public bool IsClimbing => SectionName == GamesAlias.Climbing;
    }
}