using System;
using System.Collections.Generic;
using chargebook.models;

namespace ChargeBook.models.simulation.simulationLog.snapshot {
    public class SnapshotChargeStation {
        public double usedChargePower { get; set; }
        public int occupiedChargePoints { get; set; }
        public List<SnapshotChargePoint> chargePoints { get; set; }

        public SnapshotChargeStation() {
            chargePoints = new List<SnapshotChargePoint>();
            usedChargePower = 0;
            occupiedChargePoints = 0;
        }

        public void adjustSnapshot(int pointIndex, int id, int usedPower,
            ConnectorType connectorType) {
            if (usedPower > 0) {
                usedChargePower += usedPower;
                occupiedChargePoints++;
            } else {
                if ((occupiedChargePoints - 1) >= 0) {
                    occupiedChargePoints--;
                }
                if ((usedChargePower + usedPower) >= 0) {
                    usedChargePower += usedPower;
                } else {
                    usedChargePower = 0;
                }
            }
            if (chargePoints.Count > pointIndex) {
                chargePoints[pointIndex].adjustSnapshot(id, usedPower,
                    connectorType);
            }
        }
    }
}