using System;
using CmsApp.Helpers;

namespace CmsApp.Models
{
    public class ActivityFormPublish
    {
        public string Image { get; set; }

        public int ActivityId { get; set; }
        public int? UnionId { get; set; }

        public DateTime? ActivityEndDate { get; set; }

        public string FormName { get; set; }
        public string FormDescription { get; set; }

        public string Body { get; set; }
        public ActivityFormType FormType { get; set; }
        public bool RestrictCustomPricesToOne { get; set; }
        public bool DoNotRestrictCustomPrices { get; set; }

        public bool AlternativePlayerIdentity { get; set; }

        public bool ForbidToChangeNameForExistingPlayers { get; set; }

        public CultEnum Culture { get; set; }
    }
}