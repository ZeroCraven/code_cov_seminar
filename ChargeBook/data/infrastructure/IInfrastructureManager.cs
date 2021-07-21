using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using chargebook.models.infrastructure;

namespace chargebook.data.infrastructure {
    public interface IInfrastructureManager {
        public ReadOnlyDictionary<string, ChargeStationType> possibleChargeStationTypes { get; }
        public Infrastructure getInfrastructureByLocation(string locationName);
        public IReadOnlyList<string> getLocationNames();
        public void setInfrastructureSettings(string location, int beginBuffer, int endBuffer, int minCcs, int minChademo, int minType2);
        public void createLocation(string location, TimeZoneInfo timeZone);
        public void deleteLocation(string locationName);
        public void createChargeGroup(string locationName, string chargeGroupName, double maxChargePower);
        public void deleteChargeGroup(string locationName, string chargeGroupName);
        public void createChargeStation(string locationName, string chargeGroupName, string chargeStationName, string chargeStationTypeName);
        public void deleteChargeStation(string locationName, string chargeGroupName, string chargeStationName);
        public IList<string> getInfrastructureToDistribute();
    }
}