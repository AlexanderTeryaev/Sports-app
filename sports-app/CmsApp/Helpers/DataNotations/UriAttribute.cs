using System;
using System.ComponentModel.DataAnnotations;
using Resources;

namespace CmsApp.Helpers.DataNotations
{
    public class UriAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var valid = Uri.TryCreate(Convert.ToString(value), UriKind.Absolute, out _);

            return valid
                ? ValidationResult.Success
                : new ValidationResult(Messages.InvalidURIAddress);
        }
    }
}