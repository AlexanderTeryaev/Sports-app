using System.Collections.Generic;
using AppModel;
using System.ComponentModel.DataAnnotations;
using System;

namespace CmsApp.Models
{
    public class EditRegionalViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? SectionId { get; set; }
        public string SectionName { get; set; }
        public int? UnionId { get; set; }
        public string UnionName { get; set; }
        public int? SeasonId { get; set; }
        public int CurrentSeasonId { get; set; }
        public string CurrentSeasonName { get; set; }
        public IEnumerable<Season> Seasons { get; set; }
        public bool IsClubTrainingEnabled { get; set; }
        public bool SectionIsIndividual { get; internal set; }
        public int? ParentClubId { get; set; }
        public string ParentClubTitle { get; set; }
        public int? ParentClubSectionId { get; set; }
        public int? SportId { get; set; }
        public bool CanEdit { get; set; }
        private bool isCatchBall { get; set; }
        public bool IsCatchBall
        {
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
    }

    public class RegionalDetailsForm : Regional
    {
        public CultEnum Culture { get; set; }
        public string UnionFormTitle { get; set; }        
      //  public string AppLogin { get; set; }
      //  public string AppPassword { get; set; }
       // public IEnumerable<UnionFormModel> UnionForms { get; set; }

    }
}