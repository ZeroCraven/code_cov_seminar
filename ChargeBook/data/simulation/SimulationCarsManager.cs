using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using chargebook.models;
using Newtonsoft.Json;

namespace chargebook.data.simulation {
    public class SimulationCarsManager : IDisposable {
        private Dictionary<string, Car> cars;
        private readonly ManagerPersistor persistor;

        public SimulationCarsManager(string pathToSimulationCarsFile) {
            var self = this;
            persistor = new ManagerPersistor(() => {
                string jsonString;
                lock (self) {
                    jsonString = JsonConvert.SerializeObject(cars, Formatting.Indented);
                }
                File.WriteAllText(pathToSimulationCarsFile, jsonString, Encoding.UTF8);
            });
            cars = readInitialSimulationCars(pathToSimulationCarsFile);
        }

        private static Dictionary<string, Car> readInitialSimulationCars(string path) {
            try {
                return JsonConvert.DeserializeObject<Dictionary<string, Car>>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (FileNotFoundException) {
                return new Dictionary<string, Car>();
            }
        }

        public void addCar(Car car) {
            lock (this) {
                if (cars.ContainsKey(car.name)) {
                    throw new ArgumentException();
                }
                cars.Add(car.name, car);
            }
            persistor.notifyChange();
        }

        public void deleteCar(string carName) {
            lock (this) {
                if (cars.ContainsKey(carName)) {
                    cars.Remove(carName);
                    persistor.notifyChange();
                }
            }
        }

        public List<Car> getCars() {
            lock (this) {
                return cars.Values.ToList();
            }
        }

        public bool existsCar(string carName) {
            lock (this) {
                return cars.ContainsKey(carName);
            }
        }

        public void Dispose() {
            persistor.Dispose();
        }
    }
}