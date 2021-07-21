using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using chargebook.data.user;
using chargebook.models.infrastructure;

namespace chargebook.data.infrastructure {
    public class InfrastructureManager : IInfrastructureManager {
        public IDictionary<string, Infrastructure> locations { get; }
        public ReadOnlyDictionary<string, ChargeStationType> possibleChargeStationTypes { get; }

        public InfrastructureManager(IDictionary<string, ChargeStationType> possibleChargeStationTypes,
            IDictionary<string, Infrastructure> initialLocations = null) {
            initialLocations ??= new Dictionary<string, Infrastructure>();
            locations = initialLocations;
            this.possibleChargeStationTypes = new ReadOnlyDictionary<string, ChargeStationType>(possibleChargeStationTypes);
        }

        public virtual Infrastructure getInfrastructureByLocation(string location) {
            lock (this) {
                if (!locations.ContainsKey(location)) {
                    throw new InfrastructurePartNotFoundException($"der Standort {location} existiert nicht");
                }
                return locations[location];
            }
        }

        public IReadOnlyList<string> getLocationNames() {
            lock (this) {
                return new List<string>(locations.Keys).AsReadOnly();
            }
        }

        public virtual void setInfrastructureSettings(string locationName, int beginBuffer, int endBuffer, int minCcs, int minChademo, int minType2) {
            lock (this) {
                if (!locations.ContainsKey(locationName)) {
                    throw new InfrastructurePartNotFoundException($"Der Standort \"{locationName}\" existiert nicht");
                }
                locations[locationName].infrastructureSettings = new InfrastructureSettings() {
                    beginBuffer = beginBuffer,
                    bookingDistributorActivationTime = TimeSpan.FromHours(1),
                    endBuffer = endBuffer,
                    minReservedCCSConnectors = minCcs,
                    minReservedChademo = minChademo,
                    minReservedType2 = minType2,
                };
            }
        }


        public virtual void createLocation(string location, TimeZoneInfo timeZone) {
            lock (this) {
                if (locations.ContainsKey(location)) {
                    throw new InfrastructurePartAlreadyExistsException($"Der Standort \"{location}\" existiert bereits");
                }
                locations.Add(location, new Infrastructure(timeZone));
            }
        }

        public virtual void deleteLocation(string locationName) {
            lock (this) {
                if (!locations.ContainsKey(locationName)) {
                    throw new InfrastructurePartNotFoundException($"Der Standort \"{locationName}\" existiert nicht");
                }
                locations.Remove(locationName);
            }
        }

        public virtual void createChargeGroup(string locationName, string chargeGroupName, double maxChargePower) {
            lock (this) {
                if (!locations.ContainsKey(locationName)) {
                    throw new InfrastructurePartNotFoundException($"Der Standort \"{locationName}\" existiert nicht");
                }
                var location = locations[locationName];
                if (location.chargeGroups.ContainsKey(chargeGroupName)) {
                    throw new InfrastructurePartAlreadyExistsException($"Die Ladegruppe \"{chargeGroupName}\" existiert bereits");
                }
                location.chargeGroups.Add(chargeGroupName, new ChargeGroup() {
                    maxChargePower = maxChargePower
                });
            }
        }


        public virtual void deleteChargeGroup(string locationName, string chargeGroupName) {
            lock (this) {
                if (!locations.ContainsKey(locationName)) {
                    throw new InfrastructurePartNotFoundException($"Der Standort \"{locationName}\" existiert nicht");
                }
                var location = locations[locationName];
                if (!location.chargeGroups.ContainsKey(chargeGroupName)) {
                    throw new InfrastructurePartNotFoundException($"Die Ladegruppe \"{chargeGroupName}\" existiert nicht");
                }
                location.chargeGroups.Remove(chargeGroupName);
            }
        }

        public virtual void createChargeStation(string locationName, string chargeGroupName, string chargeStationName, string chargeStationTypeName) {
            lock (this) {
                if (!locations.ContainsKey(locationName)) {
                    throw new InfrastructurePartNotFoundException($"Der Standort \"{locationName}\" existiert nicht");
                }
                var location = locations[locationName];
                if (!location.chargeGroups.ContainsKey(chargeGroupName)) {
                    throw new InfrastructurePartNotFoundException($"Die Ladegruppe \"{chargeGroupName}\" existiert nicht");
                }
                var chargeGroup = location.chargeGroups[chargeGroupName];
                if (chargeGroup.chargeStations.ContainsKey(chargeStationName)) {
                    throw new InfrastructurePartAlreadyExistsException($"Die Ladestation \"{chargeStationName}\" existiert bereits");
                }
                chargeGroup.chargeStations.Add(chargeStationName, possibleChargeStationTypes[chargeStationTypeName]);
            }
        }

        public virtual void deleteChargeStation(string locationName, string chargeGroupName, string chargeStationName) {
            lock (this) {
                if (!locations.ContainsKey(locationName)) {
                    throw new InfrastructurePartNotFoundException($"Der Standort \"{locationName}\" existiert nicht");
                }
                var location = locations[locationName];
                if (!location.chargeGroups.ContainsKey(chargeGroupName)) {
                    throw new InfrastructurePartNotFoundException($"Die Ladegruppe \"{chargeGroupName}\" existiert nicht");
                }
                var chargeGroup = location.chargeGroups[chargeGroupName];
                if (!chargeGroup.chargeStations.ContainsKey(chargeStationName)) {
                    throw new InfrastructurePartNotFoundException($"Die Ladestation \"{chargeStationName}\" existiert nicht");
                }
                chargeGroup.chargeStations.Remove(chargeStationName);
            }
        }

        public IList<string> getInfrastructureToDistribute() {
            lock (this) {
                return new List<string>(locations
                    .Where(pair => (TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pair.Value.timeZone).AddMinutes(5).Hour
                                    == pair.Value.infrastructureSettings.bookingDistributorActivationTime.Hours))
                    .Select(pair => pair.Key)).AsReadOnly();
            }
        }
    }

    #region InfrastructureExceptions

    public class InfrastructurePartNotFoundException : Exception {
        public InfrastructurePartNotFoundException(string message) : base(message) { }
    }

    public class InfrastructurePartAlreadyExistsException : Exception {
        public InfrastructurePartAlreadyExistsException(string message) : base(message) { }
    }

    #endregion

}