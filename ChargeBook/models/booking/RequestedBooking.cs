using System.Collections.Generic;

namespace ChargeBook.models.booking {
    public class RequestedBooking : Booking {
        public List<TimePeriod> timePeriods { get; set; }
        public bool denied = false;
        public int priority = 0;

        public RequestedBooking() {
            timePeriods = new List<TimePeriod>();
        }

        public override Booking deepCopy() {
            return new RequestedBooking() {
                car = car,
                denied = denied,
                location = location,
                email = email,
                timePeriods = timePeriods,
                startSoC = startSoC,
                targetSoC = targetSoC,
                priority = priority,
            };
        }
    }
}