using System;
using System.ComponentModel.DataAnnotations;
using CmsApp.Helpers.DataNotations;

namespace CmsApp.Models
{
    public class CompetitionDisciplineViewModel
    {
        public int Id { get; set; }
        public int CompetitionId { get; set; }
       
        public string CategoryId { get; set; }

        public string DistanceId { get; set; }

        public string DisciplineExpertiseId { get; set; }

        [RequiredIfUsed(nameof(IsRowing))]
        public string BoatTypesId { get; set; }

        [RequiredIfUsed(nameof(UseAllProps))]
        public int? DisciplineId { get; set; }
        public int? MaxSportsmen { get; set; }
        public double? MinResult { get; set; }
        public DateTime? StartTime { get; set; }
        public bool IsResultsManualyRanked { get; set; }
        public bool UseAllProps { get; set; } = true;
        public bool IsSwimming { get; set; }
        public bool IsRowing { get; set; }
        public bool IsBicycle { get; set; }
        public bool IsClimbing { get; set; }

        public string CompetitionRecord { get; set; }
        public int? Format { get; set; }
        public bool IncludeRecordInStartList { get; set; }
    }

    public class UpdateDisciplineRegistrationResultForm
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int CompetitionResultId { get; set; }
        public string Heat { get; set; }
        public int? Lane { get; set; }
        public float? Wind { get; set; }
        public int Rank { get; set; }
        public int Format { get; set; }
        public string Result1 { get; set; }
        public string Result2 { get; set; }
        public string Result3 { get; set; }
        public string Result4 { get; set; }
    }
}