using chargebook.models;

namespace ChargeBook.models.booking {
    public class ApprovedBooking : Booking {
        public string chargeGroupName { get; set; }
        public string chargeStationName { get; set; }
        public TimePeriod timePeriod { get; set; }
        public ApprovedBookingStatus status { get; set; }
        public int chargePointIndex { get; set; }

        public ConnectorType connectorType { get; set; }

        public override Booking deepCopy() {
            return new ApprovedBooking() {
                car = car,
                chargeGroupName = chargeGroupName,
                chargePointIndex = chargePointIndex,
                chargeStationName = chargeStationName,
                connectorType = connectorType,
                email = email,
                location = location,
                status = status,
                startSoC = startSoC,
                targetSoC = targetSoC,
                timePeriod = timePeriod
            };
        }
    }
}