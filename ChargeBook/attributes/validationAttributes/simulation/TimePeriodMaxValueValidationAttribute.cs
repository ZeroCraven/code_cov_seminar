using System;
using System.ComponentModel.DataAnnotations;
using ChargeBook.models.booking;

namespace ChargeBook.attributes.validationAttributes.simulation {
    public class TimePeriodMaxValueValidationAttribute : ValidationAttribute {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            if (!(value is TimePeriod timePeriod)) {
                return new ValidationResult(ErrorMessage);
            }
            if (timePeriod.endTime - timePeriod.startTime > TimeSpan.FromDays(30)) {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}