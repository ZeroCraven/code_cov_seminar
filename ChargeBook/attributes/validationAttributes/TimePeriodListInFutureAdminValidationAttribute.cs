using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ChargeBook.models.booking;

namespace ChargeBook.attributes.validationAttributes {
    public class TimePeriodListInFutureAdminValidationAttribute : ValidationAttribute {

        private string status;

        public TimePeriodListInFutureAdminValidationAttribute(string status) {
            this.status = status;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            if (validationContext.ObjectType.GetProperty(status) == null) {
                return new ValidationResult("Der Status darf nicht leer sein");
            }
            string usedStatus =
                (string) validationContext.ObjectType.GetProperty(status).GetValue(validationContext.ObjectInstance);
            if (usedStatus == "angefordert" || usedStatus == "angenommen") {
                if (value is List<TimePeriod> list && list.Count > 0 &&
                    list.All(timePeriod => timePeriod.startTime.Subtract(timePeriod.endTime) <= TimeSpan.Zero)) {
                    return ValidationResult.Success;
                } else {
                    if (usedStatus == "angefordert") {
                        return new ValidationResult("Die angegebenen Zeiträume müssen in der Zukunft liegen");
                    } else {
                        return new ValidationResult("Der angegebene Zeitraum muss in der Zukunft liegen");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}