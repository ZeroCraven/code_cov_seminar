using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using chargebook.models.infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;

namespace chargebook.data.infrastructure {
    public class PersistentInfrastructureManager : InfrastructureManager, IDisposable {
        private readonly ManagerPersistor persistor;

        public PersistentInfrastructureManager(IDictionary<string, ChargeStationType> possibleChargeStationTypes,
            string pathToLocationsFile) : base(possibleChargeStationTypes, readInitialLocations(pathToLocationsFile)) {
            var self = this;
            persistor = new ManagerPersistor(() => {
                string jsonString;
                lock (self) {
                    jsonString = JsonConvert.SerializeObject(locations, Formatting.Indented);
                }
                File.WriteAllText(pathToLocationsFile, jsonString, Encoding.UTF8);
            });
        }

        public void Dispose() {
            persistor.Dispose();
        }

        private static Dictionary<string, Infrastructure> readInitialLocations(string path) {
            try {
                return JsonConvert.DeserializeObject<Dictionary<string, Infrastructure>>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (FileNotFoundException) {
                return new Dictionary<string, Infrastructure>();
            }
        }

        public static Dictionary<string, ChargeStationType> readPossibleChargeStationTypes(string path) {
            try {
                return JsonConvert.DeserializeObject<Dictionary<string, ChargeStationType>>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (FileNotFoundException) {
                return new Dictionary<string, ChargeStationType>();
            }
        }

        public override void createLocation(string location, TimeZoneInfo timeZone) {
            base.createLocation(location, timeZone);
            persistor.notifyChange();
        }

        public override void deleteLocation(string locationName) {
            base.deleteLocation(locationName);
            persistor.notifyChange();
        }

        public override void createChargeGroup(string locationName, string chargeGroupName, double maxChargePower) {
            base.createChargeGroup(locationName, chargeGroupName, maxChargePower);
            persistor.notifyChange();
        }

        public override void deleteChargeGroup(string locationName, string chargeGroupName) {
            base.deleteChargeGroup(locationName, chargeGroupName);
            persistor.notifyChange();
        }

        public override void createChargeStation(string locationName, string chargeGroupName, string chargeStationName,
            string chargeStationTypeName) {
            base.createChargeStation(locationName, chargeGroupName, chargeStationName, chargeStationTypeName);
            persistor.notifyChange();
        }

        public override void deleteChargeStation(string locationName, string chargeGroupName, string chargeStationName) {
            base.deleteChargeStation(locationName, chargeGroupName, chargeStationName);
            persistor.notifyChange();
        }

        public override Infrastructure getInfrastructureByLocation(string location) {
            return base.getInfrastructureByLocation(location).deepCopy();
        }

        public override void setInfrastructureSettings(string locationName, int beginBuffer, int endBuffer, int minCcs, int minChademo,
            int minType2) {
            base.setInfrastructureSettings(locationName, beginBuffer, endBuffer, minCcs, minChademo, minType2);
            persistor.notifyChange();
        }
    }
}