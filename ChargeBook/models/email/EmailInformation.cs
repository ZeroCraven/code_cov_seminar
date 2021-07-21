using ChargeBook.models.booking;
using chargebook.models.infrastructure;

namespace chargebook.models.email {
    public class EmailInformation {
        public string name { get; set; }
        public Car car { get; set; }
        public TimePeriod timePeriod { get; set; }
        public string location { get; set; }
        public string chargeStation { get; set; }
    }
}