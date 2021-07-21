using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using chargebook.data.infrastructure;
using chargebook.data.user;
using chargebook.models.infrastructure;
using ChargeBook.models.simulation;
using ChargeBook.models.simulation.simulationLog;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace chargebook.data.simulation {
    public class PartialSimulationCache {
        private IDictionary<string, ChargeStationType> possibleChargeStationTypes;
        private Dictionary<string, Simulation> simulations;

        public PartialSimulationCache(IDictionary<string, ChargeStationType> possibleChargeStationTypes) {
            this.possibleChargeStationTypes = possibleChargeStationTypes;
            simulations = new Dictionary<string, Simulation>();
        }

        public void createSimulation(string email, string simulationLocationName, IReadOnlyDictionary<string, int> possiblePriorityRoles) {
            InfrastructureManager infrastructureManager = new InfrastructureManager(possibleChargeStationTypes);
            infrastructureManager.createLocation(simulationLocationName, TimeZoneInfo.Utc);
            simulations[email] = new Simulation(possiblePriorityRoles, infrastructureManager, email);
        }

        public void importInfrastructure(string email, string simulationLocationName, Infrastructure infrastructure,
            IReadOnlyDictionary<string, int> possiblePriorityRoles) {
            Dictionary<string, Infrastructure> importedInfrastructureDictionary = new Dictionary<string, Infrastructure>();
            importedInfrastructureDictionary.Add(simulationLocationName, infrastructure);
            InfrastructureManager infrastructureManager = new InfrastructureManager(possibleChargeStationTypes, importedInfrastructureDictionary);
            if (simulations.ContainsKey(email)) {
                simulations[email].infrastructureManager = infrastructureManager;
            } else {
                simulations[email] = new Simulation(possiblePriorityRoles, infrastructureManager, email);
            }
        }

        public Simulation getSimulationByCreatorEmail(string email) {
            if (simulations.ContainsKey(email)) {
                return simulations[email];
            }
            return null;
        }

        public void deleteSimulation(string email) {
            simulations[email] = null;
        }
    }
}