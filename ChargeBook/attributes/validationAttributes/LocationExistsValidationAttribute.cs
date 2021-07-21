using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using chargebook.data.infrastructure;

namespace ChargeBook.attributes.validationAttributes {
    public class LocationExistsValidationAttribute : ValidationAttribute {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            IInfrastructureManager infrastructureManager = (IInfrastructureManager) validationContext.GetService(typeof(IInfrastructureManager));
            if (value is string location && infrastructureManager.getLocationNames().Contains(location)) {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}