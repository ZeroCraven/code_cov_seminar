using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ChargeBook.models.booking;

namespace ChargeBook.attributes.validationAttributes {
    public class TimePeriodInFutureValidationAttribute : ValidationAttribute {
        public override bool IsValid(object value) {
            return value is TimePeriod timePeriod && timePeriod.startTime.Subtract(DateTime.UtcNow) > TimeSpan.Zero;
        }
    }
}