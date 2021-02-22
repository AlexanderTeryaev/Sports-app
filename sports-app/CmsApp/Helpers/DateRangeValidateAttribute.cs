using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsApp.Helpers
{
    public class DateRangeValidateAttribute : ValidationAttribute
    {
        public int MaxYearsAgo { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            value = (DateTime?)value;
            if (value != null)
            {
                if (DateTime.Now.AddYears(-MaxYearsAgo).CompareTo(value) <= 0 && DateTime.Now.CompareTo(value) >= 0)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(String.Concat(Resources.Messages.DateMustBeWithin, MaxYearsAgo,
                        Resources.Messages.Years));
                }
            }

            return ValidationResult.Success;
        }
    }
}