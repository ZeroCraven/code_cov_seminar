using System;
using System.Collections.Generic;
using System.Linq;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using ChargeBook.models.booking;
using chargebook.models.infrastructure;
using ChargeBook.models.simulation.settings;
using ChargeBook.models.simulation.simulationLog.snapshot;
using chargebook.viewModels;

namespace ChargeBook.models.simulation.simulationLog {
    public class SimulationLog {
        public string creatorEmail { get; set; }
        public DateTime startTime { get; set; }

        public Scenario scenario { get; set; }
        private SimulationSnapshot snapshot;
        public List<SimulationSnapshot> snapshots;
        private Dictionary<int, Booking> simulationBookings;
        public Dictionary<int, BookingsViewModel> viewSimulationBookings;
        private Infrastructure simulatedInfrastructure;
        public Statistics statistics;

        private List<Tuple<BookingsViewModel, BookingsViewModel>> adHocRequestedBookingsDelta =
            new List<Tuple<BookingsViewModel, BookingsViewModel>>();


        public SimulationLog() {
            snapshots = new List<SimulationSnapshot>();
            simulationBookings = new Dictionary<int, Booking>();
        }
        
        public SimulationLog(Infrastructure infrastructure) {
            snapshots = new List<SimulationSnapshot>();
            simulationBookings = new Dictionary<int, Booking>();
            simulatedInfrastructure = infrastructure;
            initiateSnapshot();
        }

        public void createScenario(Infrastructure infrastructure, SimulationSettings settings) {
            scenario = new Scenario() {
                simulatedInfrastructure = infrastructure, simulationSettings = settings,
            };
        }

        public void addRequestedAdHoc(int id, RequestedBooking requestedBooking) {
            adHocRequestedBookingsDelta = new List<Tuple<BookingsViewModel, BookingsViewModel>>();
            adHocRequestedBookingsDelta.Add(new Tuple<BookingsViewModel, BookingsViewModel>
            (null,
                BookingsViewModel.fromBooking(id, requestedBooking)));
            simulationBookings.Add(id, requestedBooking.deepCopy());
        }

        private void initiateSnapshot() {
            snapshot = new SimulationSnapshot() {
                chargeGroups = new Dictionary<string, SnapshotChargeGroup>(),
            };

            foreach (var chargeGroupKeyValue in simulatedInfrastructure.chargeGroups) {
                snapshot.chargeGroups.Add(chargeGroupKeyValue.Key, new SnapshotChargeGroup());
                foreach (var chargeStationKeyValue in chargeGroupKeyValue.Value.chargeStations) {
                    snapshot.chargeGroups[chargeGroupKeyValue.Key].chargeStations.Add(chargeStationKeyValue.Key, new SnapshotChargeStation());
                    foreach (var chargePoint in chargeStationKeyValue.Value.chargePoints) {
                        snapshot.chargeGroups[chargeGroupKeyValue.Key].chargeStations[chargeStationKeyValue.Key].chargePoints
                            .Add(new SnapshotChargePoint());
                    }
                }
            }
        }

        public void log(IInfrastructureManager infrastructureManager, IBookingManager bookingManager, DateTime time) {
            List<Tuple<BookingsViewModel, BookingsViewModel>> deltaBookings = new List<Tuple<BookingsViewModel, BookingsViewModel>>();
            var newBookings = new Dictionary<int, Booking>(bookingManager.getAllBookings());
            var newRequestedBookings = newBookings.Where(pair => pair.Value is RequestedBooking);
            foreach (var newKeyValuePair in newRequestedBookings) {
                if (simulationBookings.Keys.Contains(newKeyValuePair.Key)) {
                    var bookingOld = (RequestedBooking) simulationBookings[newKeyValuePair.Key];
                    if (((RequestedBooking) newKeyValuePair.Value).denied != bookingOld.denied) {
                        deltaBookings.Add(new Tuple<BookingsViewModel, BookingsViewModel>
                        (BookingsViewModel.fromBooking(newKeyValuePair.Key, bookingOld),
                            BookingsViewModel.fromBooking(newKeyValuePair.Key, newKeyValuePair.Value)));
                    }
                } else {
                    deltaBookings.Add(new Tuple<BookingsViewModel, BookingsViewModel>(null,
                        BookingsViewModel.fromBooking(newKeyValuePair.Key, newKeyValuePair.Value)));
                }
            }


            var approvedBookings = newBookings.Where(pair => pair.Value is ApprovedBooking);
            foreach (var newKeyValuePair in approvedBookings) {
                if (simulationBookings[newKeyValuePair.Key] is RequestedBooking) {
                    deltaBookings.Add(new Tuple<BookingsViewModel, BookingsViewModel>
                    (BookingsViewModel.fromBooking(newKeyValuePair.Key, simulationBookings[newKeyValuePair.Key]),
                        BookingsViewModel.fromBooking(newKeyValuePair.Key, newKeyValuePair.Value)));
                    continue;
                }
                var bookingOld = (ApprovedBooking) simulationBookings[newKeyValuePair.Key];
                var bookingNew = (ApprovedBooking) newKeyValuePair.Value;

                if (bookingNew.status != bookingOld.status) {
                    deltaBookings.Add(new Tuple<BookingsViewModel, BookingsViewModel>(BookingsViewModel.fromBooking(newKeyValuePair.Key, bookingOld),
                        BookingsViewModel.fromBooking(newKeyValuePair.Key, newKeyValuePair.Value)));

                    if (bookingNew.status == ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED) {
                        snapshot.adjustSnapshot(bookingNew.chargeGroupName, bookingNew.chargeStationName, bookingNew.chargePointIndex,
                            newKeyValuePair.Key,
                            (int) (
                                Math.Min(
                                    simulatedInfrastructure.chargeGroups[bookingNew.chargeGroupName]
                                        .chargeStations[bookingNew.chargeStationName].chargePoints[bookingNew.chargePointIndex]
                                        .connectors[bookingNew.connectorType],
                                    Convert.ToDouble(bookingNew.car.connectors[bookingNew.connectorType]))),
                            bookingNew.connectorType);
                    }
                    if (bookingNew.status == ApprovedBookingStatus.CHARGE_END_CONFIRMED) {
                        snapshot.adjustSnapshot(bookingNew.chargeGroupName, bookingNew.chargeStationName, bookingNew.chargePointIndex,
                            newKeyValuePair.Key,
                            (int) (-1 * Math.Min(simulatedInfrastructure.chargeGroups[bookingNew.chargeGroupName]
                                    .chargeStations[bookingNew.chargeStationName].chargePoints[bookingNew.chargePointIndex]
                                    .connectors[bookingNew.connectorType],
                                Convert.ToDouble(bookingNew.car.connectors[bookingNew.connectorType]))),
                            bookingNew.connectorType);
                    }
                }
            }
            simulationBookings = new Dictionary<int, Booking>();
            foreach (var keyValuePair in newBookings) {
                simulationBookings.Add(keyValuePair.Key, keyValuePair.Value.deepCopy());
            }


            var newSnapshot = snapshot.deepCopy();
            newSnapshot.dateTime = time;
            newSnapshot.deltaBookings = deltaBookings;


            foreach (var tuple in adHocRequestedBookingsDelta) {
                newSnapshot.deltaBookings.Add(tuple);
            }

            newSnapshot.deltaBookings.Sort((x, y) => {
                if (x.Item1 == null && y.Item1 == null) {
                    return 0;
                }
                if (x.Item1 == null && y.Item1 != null) {
                    return -1;
                }
                if (x.Item1 != null && y.Item1 == null) {
                    return 1;
                }
                if (x.Item1 != null && y.Item1 != null) {
                    return 0;
                }
                return 0;
            });

            adHocRequestedBookingsDelta = new List<Tuple<BookingsViewModel, BookingsViewModel>>();
            snapshots.Add(newSnapshot);
        }


        public void endLog() {
            viewSimulationBookings = new Dictionary<int, BookingsViewModel>();
            foreach (var keyValuePair in simulationBookings) {
                viewSimulationBookings[keyValuePair.Key] = BookingsViewModel.fromBooking(keyValuePair.Key, keyValuePair.Value);
            }
            statistics = new Statistics() {
                countApprovedBookings = simulationBookings.Count(x => x.Value is ApprovedBooking),
                countDeniedBookings = simulationBookings.Count(x => x.Value is RequestedBooking rb && rb.denied),
                peakOccupancy = snapshots.Max(x => x.occupiedChargePoints),
                peakWorkload = snapshots.Max(x => x.usedPower),
                requestedEnergy = (long) simulationBookings.Where(x => x.Value is ApprovedBooking || (x.Value is RequestedBooking rb && rb.denied))
                    .Sum(x => x.Value.getRequestedChargeEnergy()),
                suppliedEnergy = (long) simulationBookings.Where(x => x.Value is ApprovedBooking)
                    .Sum(x => x.Value.getRequestedChargeEnergy()),
                countApprovedAdHocBookings = simulationBookings.Count(x => x.Value.email.StartsWith("AdHoc") && x.Value is ApprovedBooking),
                countDeniedAdHocBookings = simulationBookings.Count(x => x.Value.email.StartsWith("AdHoc") &&
                                                                         x.Value is RequestedBooking rb && rb.denied)
            };
        }


    }
}