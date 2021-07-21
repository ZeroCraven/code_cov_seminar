using System;
using System.Collections.Generic;
using chargebook.models;

namespace ChargeBook.models.simulation.simulationLog.snapshot {
    public class SnapshotChargeGroup {
        public double usedPower { get; set; }
        public int occupiedChargePoints { get; set; }
        public Dictionary<string, SnapshotChargeStation> chargeStations { get; set; }

        public SnapshotChargeGroup() {
            chargeStations = new Dictionary<string, SnapshotChargeStation>();
            usedPower = 0;
            occupiedChargePoints = 0;
        }

        public void adjustSnapshot(string stationName, int pointIndex, int id, int usedPower,
            ConnectorType connectorType) {
            if (usedPower > 0) {
                this.usedPower += usedPower;
                occupiedChargePoints++;
            } else {
                if ((occupiedChargePoints - 1) >= 0) {
                    occupiedChargePoints--;
                }
                if ((this.usedPower + usedPower) >= 0) {
                    this.usedPower += usedPower;
                } else {
                    usedPower = 0;
                }
            }
            if (chargeStations.ContainsKey(stationName)) {
                chargeStations[stationName].adjustSnapshot(pointIndex, id, usedPower,
                    connectorType);
            }
        }
    }
}