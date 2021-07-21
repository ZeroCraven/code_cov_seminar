using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using chargebook.models;
using ChargeBook.models.simulation;
using ChargeBook.models.simulation.settings;
using ChargeBook.models.simulation.simulationLog;
using ChargeBook.viewModels.simulationViewModels;
using Newtonsoft.Json;

namespace chargebook.data.simulation {
    public class SimulationLogManager : IDisposable {

        private Dictionary<int, SimulationLog> simulationLogs;
        private readonly ManagerPersistor persistor;

        public SimulationLogManager(string pathToSimulationLogsFile) {
            var self = this;
            persistor = new ManagerPersistor(() => {
                string jsonString;
                lock (self) {
                    jsonString = JsonConvert.SerializeObject(simulationLogs);
                }
                File.WriteAllText(pathToSimulationLogsFile, jsonString, Encoding.UTF8);
            });
            simulationLogs = readInitialSimulationLogs(pathToSimulationLogsFile);
            if (simulationLogs.Count > 0) {
                Simulation.idCounter = simulationLogs.Keys.Max();
            }
        }


        private static Dictionary<int, SimulationLog> readInitialSimulationLogs(string path) {
            try {
                return JsonConvert.DeserializeObject<Dictionary<int, SimulationLog>>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (FileNotFoundException) {
                return new Dictionary<int, SimulationLog>();
            }
        }

        public void Dispose() {
            persistor.Dispose();
        }


        public void addSimulationLog(int id, SimulationLog simulationLog) {
            lock (this) {
                if (simulationLogs.ContainsKey(id)) {
                    throw new ArgumentException();
                }
                simulationLogs.Add(id, simulationLog);
            }
            persistor.notifyChange();
        }

        public void deleteSimulationLog(int id) {
            lock (this) {
                if (!simulationLogs.ContainsKey(id)) {
                    throw new ArgumentException();
                }
                simulationLogs.Remove(id);
            }
            persistor.notifyChange();
        }

        public IEnumerable<SimulationPreviewViewModel> getHistory() {
            lock (this) {
                IEnumerable<SimulationPreviewViewModel> history = simulationLogs.Select(x =>
                    new SimulationPreviewViewModel() {
                        id = x.Key,
                        creatorEmail = x.Value.creatorEmail,
                        generalSettings = x.Value.scenario.simulationSettings.generalSettings,
                        statistics = x.Value.statistics,
                        startTime = x.Value.startTime,
                    }).ToList().OrderByDescending(h => h.startTime);
                return history;
            }
        }

        public SimulationLog getSimulationLogById(int simulationLogId) {
            lock (this) {
                if (!simulationLogs.ContainsKey(simulationLogId)) {
                    throw new ArgumentException();
                }
                return simulationLogs[simulationLogId];
            }
        }

        public Scenario getScenarioBySimulationLogId(int id) {
            lock (this) {
                if (!simulationLogs.ContainsKey(id)) {
                    throw new ArgumentException();
                }
                return simulationLogs[id].scenario;
            }
        }

    }
}