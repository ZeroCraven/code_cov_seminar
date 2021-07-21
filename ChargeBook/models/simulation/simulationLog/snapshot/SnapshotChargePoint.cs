using chargebook.models;

namespace ChargeBook.models.simulation.simulationLog.snapshot {
    public class SnapshotChargePoint {
        public ConnectorType usedConnector { get; set; }
        public int usedPower { get; set; }
        public int bookingId;

        public SnapshotChargePoint() {
            usedPower = 0;
        }

        public void adjustSnapshot(int id, int usedPower,
            ConnectorType connectorType) {
            usedConnector = connectorType;
            if (this.usedPower + usedPower < 0) {
                this.usedPower = 0;
            } else {
                this.usedPower += usedPower;
            }
            this.bookingId = id;
        }

    }
}