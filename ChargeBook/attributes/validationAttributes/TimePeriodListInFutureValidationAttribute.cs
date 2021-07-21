using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ChargeBook.models.booking;

namespace ChargeBook.attributes.validationAttributes {
    public class TimePeriodListInFutureValidationAttribute : ValidationAttribute {
        public override bool IsValid(object value) {
            return value is List<TimePeriod> list && list.Count > 0 &&
                   list.All(timePeriod => timePeriod.startTime.Subtract(DateTime.UtcNow) > TimeSpan.Zero);
        }
    }
}