using System.ComponentModel.DataAnnotations;
using chargebook.data.infrastructure;

namespace ChargeBook.attributes.validationAttributes {
    public class ChargeStationTypeStringFormatValidationAttribute : ValidationAttribute {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            IInfrastructureManager infrastructureManager = (IInfrastructureManager) validationContext.GetService(typeof(IInfrastructureManager));
            if (infrastructureManager == null) {
                return new ValidationResult(ErrorMessage);
            }
            if (value is string chargeStationType && infrastructureManager.possibleChargeStationTypes.ContainsKey(chargeStationType)) {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}