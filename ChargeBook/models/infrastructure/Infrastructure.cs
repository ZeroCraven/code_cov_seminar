using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using chargebook.data.infrastructure;
using ChargeBook.models.booking;
using Newtonsoft.Json;

namespace chargebook.models.infrastructure {
    public class Infrastructure {
        public Dictionary<string, ChargeGroup> chargeGroups { get; set; }

        [JsonConverter(typeof(TimeZoneInfoJsonConverter))]
        public TimeZoneInfo timeZone { get; set; }

        public InfrastructureSettings infrastructureSettings { get; set; }

        [JsonConstructor]
        public Infrastructure() {
            chargeGroups = new Dictionary<string, ChargeGroup>();
            infrastructureSettings = new InfrastructureSettings();
        }

        public Infrastructure(TimeZoneInfo timeZone) {
            this.timeZone = timeZone;
            chargeGroups = new Dictionary<string, ChargeGroup>();
            infrastructureSettings = new InfrastructureSettings() {
                bookingDistributorActivationTime = TimeSpan.FromHours(1),
                beginBuffer = 15,
                endBuffer = 15,
                minReservedCCSConnectors = 0,
                minReservedChademo = 0,
                minReservedType2 = 0,
            };
        }

        public List<Tuple<string, string, int, double>> getAvailableChargePointsByConnectorType(ConnectorType connectorType) {
            List<Tuple<string, string, int, double>> availableChargePoints = new List<Tuple<string, string, int, double>>();
            foreach (var (chargeGroupName, chargeGroup) in chargeGroups) {
                foreach (var (chargeStationName, chargeStation) in chargeGroup.chargeStations) {
                    var index = 0;
                    foreach (var chargePoint in chargeStation.chargePoints) {
                        if (chargePoint.connectors.ContainsKey(connectorType)) {
                            availableChargePoints.Add(new Tuple<string, string, int, double>(chargeGroupName, chargeStationName, index,
                                chargePoint.connectors[connectorType]));
                        }
                        index++;
                    }
                }
            }
            return availableChargePoints;
        }

        public Infrastructure deepCopy() {
            Infrastructure infrastructure = new Infrastructure(timeZone);
            infrastructure.infrastructureSettings = new InfrastructureSettings() {
                beginBuffer = infrastructureSettings.beginBuffer,
                bookingDistributorActivationTime = infrastructureSettings.bookingDistributorActivationTime,
                endBuffer = infrastructureSettings.endBuffer,
                minReservedCCSConnectors = infrastructureSettings.minReservedCCSConnectors,
                minReservedChademo = infrastructureSettings.minReservedChademo,
                minReservedType2 = infrastructureSettings.minReservedType2,
            };
            infrastructure.chargeGroups = new Dictionary<string, ChargeGroup>();
            foreach (var chargeGroupKeyValue in chargeGroups) {
                infrastructure.chargeGroups.Add(chargeGroupKeyValue.Key, new ChargeGroup() {
                    maxChargePower = chargeGroupKeyValue.Value.maxChargePower
                });
                foreach (var chargeStationKeyValue in chargeGroupKeyValue.Value.chargeStations) {
                    infrastructure.chargeGroups[chargeGroupKeyValue.Key].chargeStations.Add(chargeStationKeyValue);
                }
            }
            return infrastructure;
        }
    }
}