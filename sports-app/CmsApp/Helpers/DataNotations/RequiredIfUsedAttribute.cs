using System.ComponentModel.DataAnnotations;

namespace CmsApp.Helpers.DataNotations
{
    public class RequiredIfUsedAttribute : ValidationAttribute
    {
        private readonly RequiredAttribute _required = new RequiredAttribute();
        private readonly string _usedUsedPropertyName;

        public RequiredIfUsedAttribute(string usedPropertyName) : base()
        {
            _usedUsedPropertyName = usedPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var usedProperty = validationContext.ObjectType.GetProperty(_usedUsedPropertyName);
            if (usedProperty == null)
                return new ValidationResult($"Unknown property: {_usedUsedPropertyName}");

            if (usedProperty.PropertyType != typeof(bool))
                return new ValidationResult($"{usedProperty.Name} has wrong type. Only bool is allowed.");

            if ((bool)usedProperty.GetValue(validationContext.ObjectInstance, null))
            {
                return _required.GetValidationResult(value, validationContext);
            }

            return null;
        }
    }
}