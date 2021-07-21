using System.Collections.Generic;

namespace chargebook.models.infrastructure {
    public class ChargePoint {
        public Dictionary<ConnectorType, double> connectors { get; set; }

        public ChargePoint() {
            connectors = new Dictionary<ConnectorType, double>();
        }
    }
}