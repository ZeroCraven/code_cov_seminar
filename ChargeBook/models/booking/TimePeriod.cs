using System;

namespace ChargeBook.models.booking {
    public class TimePeriod {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }

        public TimePeriod(DateTime d1, DateTime d2) {
            startTime = d1;
            endTime = d2;
        }

        public TimePeriod() { }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (obj is TimePeriod t2) {
                return t2.startTime.Equals(this.startTime) && t2.endTime.Equals(this.endTime);
            }
            return false;
        }

        public TimePeriod deepCopy() {
            return new TimePeriod() {
                startTime = startTime,
                endTime = endTime,
            };
        }
    }
}