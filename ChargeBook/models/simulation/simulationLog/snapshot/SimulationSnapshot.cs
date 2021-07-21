using System;
using System.Collections.Generic;
using chargebook.models;
using chargebook.viewModels;

namespace ChargeBook.models.simulation.simulationLog.snapshot {
    public class SimulationSnapshot {
        public Dictionary<string, SnapshotChargeGroup> chargeGroups { get; set; }
        public List<Tuple<BookingsViewModel, BookingsViewModel>> deltaBookings { get; set; }
        public double usedPower { get; set; }
        public int occupiedChargePoints { get; set; }

        public DateTime dateTime;

        public SimulationSnapshot() {
            chargeGroups = new Dictionary<string, SnapshotChargeGroup>();
            usedPower = 0;
            occupiedChargePoints = 0;
        }

        public void adjustSnapshot(string groupName, string stationName, int pointIndex, int id, int usedPower,
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
                    this.usedPower = 0;
                }
            }
            if (chargeGroups.ContainsKey(groupName)) {
                chargeGroups[groupName].adjustSnapshot(stationName, pointIndex, id, usedPower, connectorType);
            }
        }

        public SimulationSnapshot deepCopy() {
            SimulationSnapshot snapshot = new SimulationSnapshot() {
                chargeGroups = new Dictionary<string, SnapshotChargeGroup>(), occupiedChargePoints = occupiedChargePoints, usedPower = usedPower,
            };
            foreach (var chargeGroupKeyValue in chargeGroups) {
                snapshot.chargeGroups.Add(chargeGroupKeyValue.Key, new SnapshotChargeGroup() {
                    occupiedChargePoints = chargeGroupKeyValue.Value.occupiedChargePoints, usedPower = chargeGroupKeyValue.Value.usedPower,
                });
                foreach (var chargeStationKeyValue in chargeGroupKeyValue.Value.chargeStations) {
                    snapshot.chargeGroups[chargeGroupKeyValue.Key].chargeStations.Add(chargeStationKeyValue.Key, new SnapshotChargeStation() {
                        occupiedChargePoints = chargeStationKeyValue.Value.occupiedChargePoints,
                        usedChargePower = chargeStationKeyValue.Value.usedChargePower,
                    });
                    foreach (var chargePoint in chargeStationKeyValue.Value.chargePoints) {
                        snapshot.chargeGroups[chargeGroupKeyValue.Key].chargeStations[chargeStationKeyValue.Key].chargePoints.Add(
                            new SnapshotChargePoint() {
                                bookingId = chargePoint.bookingId, usedConnector = chargePoint.usedConnector, usedPower = chargePoint.usedPower,
                            });
                    }
                }
            }
            return snapshot;
        }
    }


}