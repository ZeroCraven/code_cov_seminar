using System;
using System.ComponentModel.DataAnnotations;
using ChargeBook.models.booking;

namespace ChargeBook.attributes.validationAttributes.simulation {
    public class TimePeriodPositiveTimeSpanSimulationValidation : ValidationAttribute {
        public override bool IsValid(object value) {
            return value is TimePeriod timePeriod && timePeriod.startTime.Subtract(timePeriod.endTime) <= TimeSpan.Zero;
        }
    }
}