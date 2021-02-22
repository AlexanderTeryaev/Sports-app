using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class WeightLiftingResultsForm
    {
        public int CompetitionRegistrationId { get; set; }
        public Nullable<int> Lifting1 { get; set; }
        public Nullable<int> Lifting2 { get; set; }
        public Nullable<int> Lifting3 { get; set; }
        public Nullable<int> Push1 { get; set; }
        public Nullable<int> Push2 { get; set; }
        public Nullable<int> Push3 { get; set; }
        public Nullable<int> Lift1Success { get; set; }
        public Nullable<int> Lift2Success { get; set; }
        public Nullable<int> Lift3Success { get; set; }
        public Nullable<int> Push1Success { get; set; }
        public Nullable<int> Push2Success { get; set; }
        public Nullable<int> Push3Success { get; set; }
        public Nullable<int> PushResult { get; set; }
        public Nullable<int> LiftingResult { get; set; }
        public Nullable<int> FinalResult { get; set; }
        public decimal? WeightDeclaration { get; set; }
        public int ChosenNextRegId { get; set; }
    }
}