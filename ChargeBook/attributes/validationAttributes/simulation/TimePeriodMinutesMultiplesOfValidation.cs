using System.ComponentModel.DataAnnotations;
using ChargeBook.models.booking;

namespace ChargeBook.attributes.validationAttributes.simulation {
    public class TimePeriodMinutesMultiplesOfValidation : ValidationAttribute {
        public int factor = 1;

        public TimePeriodMinutesMultiplesOfValidation(int factor) {
            this.factor = factor;
        }

        public override bool IsValid(object value) {
            if (!(value is TimePeriod timePeriod)) {
                return false;
            }
            if (timePeriod.startTime.Minute % factor != 0) {
                return false;
            }
            if (timePeriod.endTime.Minute % factor != 0) {
                return false;
            }
            return true;
        }
    }
}