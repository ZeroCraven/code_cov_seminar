using System.Collections.Generic;

namespace chargebook.models.infrastructure {
    public class ChargeStationType {
        public string modelName { get; set; }
        public string manufacturer { get; set; }
        public double maxChargePower { get; set; }
        public List<ChargePoint> chargePoints { get; set; }

        public ChargeStationType() {
            chargePoints = new List<ChargePoint>();
        }
    }
}