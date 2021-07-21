using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using chargebook.models;
using ChargeBook.models.booking;
using ChargeBook.models.booking.bookingExceptions;
using chargebook.models.infrastructure;
using ChargeBook.models.simulation.settings;
using ChargeBook.models.simulation.simulationLog;
using chargebook.viewModels.simulationViewModels;

namespace ChargeBook.models.simulation {
    public class Simulation {
        public static int idCounter = 0;
        public static readonly string SIMULATION_LOCATION_NAME = "simulierter Standort";

        public List<KeyValuePair<int, Booking>> bookingsFromLocation = new List<KeyValuePair<int, Booking>>();
        public IInfrastructureManager infrastructureManager { get; set; }
        private readonly IBookingManager bookingManager;

        public SimulationSettings simulationSettings;
        private SimulationLog logger;
        public int id { get; }
        private int index = 0;
        private readonly string creatorEmail;
        private DateTime simulationTime;

        public readonly List<Tuple<DateTime, RequestedBooking>> upcomingRequestedBookings = new List<Tuple<DateTime, RequestedBooking>>();
        public readonly List<Tuple<DateTime, RequestedBooking>> upcommingAdHocBookings = new List<Tuple<DateTime, RequestedBooking>>();

        private IReadOnlyDictionary<string, int> priorityRoles;

        public Simulation(IReadOnlyDictionary<string, int> priorityRoles, IInfrastructureManager infrastructureManager, string creator) {
            bookingManager = new NonPersistentBookingManager();
            this.infrastructureManager = infrastructureManager;
            id = getNextId();
            creatorEmail = creator;
            simulationSettings = new SimulationSettings() {
                generalSettings = new GeneralSettings(), bookingGenerationSettings = new List<BookingGenerationSetting>(),
            };
            simulationSettings.generalSettings.seed = Environment.TickCount;
            this.priorityRoles = priorityRoles;
        }


        public void createScenario() {
            logger.createScenario(infrastructureManager.getInfrastructureByLocation(SIMULATION_LOCATION_NAME),
                simulationSettings);
        }


        public void setBookingGenerationSettings(List<BookingGenerationSetting> generationSettings) {
            simulationSettings.bookingGenerationSettings = new List<BookingGenerationSetting>();
            foreach (var bookingGenerationSetting in generationSettings) {
                simulationSettings.bookingGenerationSettings.Add(bookingGenerationSetting);
            }
        }

        public SimulationLog getLogger() {
            return logger;
        }

        public void setGeneralSettings(SimulationGeneralSettingsViewModel generalSettings) {
            generalSettings.timePeriod.endTime = generalSettings.timePeriod.endTime.AddDays(1);
            simulationSettings.generalSettings = new GeneralSettings() {
                name = generalSettings.name,
                timePeriod = generalSettings.timePeriod,
                tickLength = generalSettings.tickLength,
                seed = generalSettings.seed.Value,
            };
        }

        private void generateRequestedBookings() {
            Random random = new Random(simulationSettings.generalSettings.seed);
            foreach (var bookingGenerationSetting in simulationSettings.bookingGenerationSettings) {
                for (int i = 0; i < bookingGenerationSetting.count; i++) {
                    var dateTime = simulationSettings.generalSettings.timePeriod.startTime.Date;
                    do {
                        if (bookingGenerationSetting.timeOfRequest < TimeSpan.FromDays(1)) {
                            if (random.NextDouble() <= bookingGenerationSetting.requestFrequency /
                                bookingGenerationSetting.bookingDays.Count) {
                                List<TimePeriod> timePeriods = new List<TimePeriod>();
                                foreach (var tuple in bookingGenerationSetting.requestedTimeSpans) {
                                    timePeriods.Add(new TimePeriod(dateTime.Date + tuple.Item1, dateTime.Date + tuple.Item2));
                                }
                                timePeriods.Sort((x, y) => x.startTime.CompareTo(y.startTime));

                                DateTime requestedAdhocDatetime;

                                if (((timePeriods[0].startTime - dateTime.Date).TotalMinutes - simulationSettings.generalSettings.tickLength) <= 0) {
                                    requestedAdhocDatetime =
                                        timePeriods[0].startTime - TimeSpan.FromMinutes(simulationSettings.generalSettings.tickLength);
                                } else {
                                    var possibleMinutes15FromMidnight = (int) (((timePeriods[0].startTime - dateTime.Date).TotalMinutes - 30) / 15);
                                    requestedAdhocDatetime = dateTime + TimeSpan.FromMinutes((random.Next() % possibleMinutes15FromMidnight) * 15);
                                }
                                int priority = 0;
                                if (priorityRoles.ContainsKey(bookingGenerationSetting.priorityRole)) {
                                    priority = priorityRoles[bookingGenerationSetting.priorityRole];
                                }
                                RequestedBooking newRequestedBooking = new RequestedBooking() {
                                    car = bookingGenerationSetting.car,
                                    denied = false,
                                    email =
                                        "AdHoc Buchung von " + bookingGenerationSetting.priorityRole,
                                    location = SIMULATION_LOCATION_NAME,
                                    startSoC = 0,
                                    targetSoC = 0 + bookingGenerationSetting.chargedEnergyInPercent,
                                    timePeriods = timePeriods,
                                    priority = priority,
                                };
                                upcommingAdHocBookings.Add(new Tuple<DateTime, RequestedBooking>(requestedAdhocDatetime, newRequestedBooking));
                            }
                        } else {
                            var dateTimeChanged = dateTime + bookingGenerationSetting.timeOfRequest;
                            if (bookingGenerationSetting.bookingDays.Contains(dateTimeChanged.DayOfWeek)) {
                                if (random.NextDouble() <= bookingGenerationSetting.requestFrequency /
                                    bookingGenerationSetting.bookingDays.Count) {
                                    DateTime requestTime = dateTime.Date.AddHours(7).AddMinutes((random.Next() % 60) * 15);
                                    List<TimePeriod> timePeriods = new List<TimePeriod>();
                                    foreach (var tuple in bookingGenerationSetting.requestedTimeSpans) {
                                        timePeriods.Add(new TimePeriod(dateTimeChanged.Date + tuple.Item1, dateTimeChanged.Date + tuple.Item2));
                                    }
                                    int priority = 0;
                                    if (priorityRoles.ContainsKey(bookingGenerationSetting.priorityRole)) {
                                        priority = priorityRoles[bookingGenerationSetting.priorityRole];
                                    }
                                    timePeriods.Sort((x, y) => x.startTime.CompareTo(y.startTime));
                                    RequestedBooking newRequestedBooking = new RequestedBooking() {
                                        car = bookingGenerationSetting.car,
                                        denied = false,
                                        email = "Buchung von " + bookingGenerationSetting.priorityRole,
                                        location = SIMULATION_LOCATION_NAME,
                                        startSoC = 0,
                                        targetSoC = 0 + bookingGenerationSetting.chargedEnergyInPercent,
                                        timePeriods = timePeriods,
                                        priority = priority,
                                    };
                                    upcomingRequestedBookings.Add(new Tuple<DateTime, RequestedBooking>(requestTime, newRequestedBooking));
                                }
                            }
                        }
                        dateTime = dateTime.AddDays(1);
                    } while ((dateTime.Date - simulationSettings.generalSettings.timePeriod.endTime.Date) < TimeSpan.Zero);
                }
            }
        }

        public bool prepareForStart() {
            if (simulationSettings.generalSettings.isNotFullySet() || infrastructureManager.getLocationNames().Count == 0 ||
                simulationSettings.bookingGenerationSettings.Any(x => x.isNotFullySet())
                || simulationSettings.bookingGenerationSettings.Count == 0) {
                return false;
            }

            simulationSettings.generalSettings.totalTicks = (int) Math.Round(
                (simulationSettings.generalSettings.timePeriod.endTime - simulationSettings.generalSettings.timePeriod.startTime)
                .TotalMinutes
                / simulationSettings.generalSettings.tickLength);
            simulationTime = simulationSettings.generalSettings.timePeriod.startTime;

            logger = new SimulationLog(infrastructureManager.getInfrastructureByLocation(SIMULATION_LOCATION_NAME)) {
                creatorEmail = creatorEmail,
            };
            foreach (var booking in bookingsFromLocation.Select(x => x.Value).Where(x => {
                if (x is RequestedBooking rb) {
                    return !rb.denied;
                }
                if (x is ApprovedBooking ab) {
                    return simulationSettings.generalSettings.timePeriod.startTime < ab.timePeriod.startTime &&
                           simulationSettings.generalSettings.timePeriod.endTime > ab.timePeriod.endTime;
                }
                return false;
            })) {
                if (booking is RequestedBooking rb) {
                    bookingManager.createRequestedBooking(rb.email, SIMULATION_LOCATION_NAME, rb.car, rb.startSoC, rb.targetSoC, rb.timePeriods,
                        rb.priority);
                }
                if (booking is ApprovedBooking ab) {
                    bookingManager.createApprovedBooking(ab.email, SIMULATION_LOCATION_NAME, ab.car, ab.startSoC, ab.targetSoC,
                        ab.timePeriod, ab.chargeGroupName, ab.chargeStationName, ab.chargePointIndex, ab.status, ab.connectorType);
                }
            }
            generateRequestedBookings();
            upcomingRequestedBookings.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            createScenario();
            return true;
        }

        public async Task runAsync() {
            logger.startTime = DateTime.UtcNow;

            for (var i = 0; i < simulationSettings.generalSettings.totalTicks; i++) {
                tick();
            }
            var timePeriod = logger.scenario.simulationSettings.generalSettings.timePeriod;
            logger.endLog();
            logger.scenario.simulationSettings.generalSettings.timePeriod = new TimePeriod(timePeriod.startTime, timePeriod.endTime.AddDays(-1));
            await Task.CompletedTask;
        }

        private void tick() {
            simulationTime = simulationTime.AddMinutes(simulationSettings.generalSettings.tickLength);

            foreach (var booking in upcommingAdHocBookings) {
                if (booking.Item1 <= simulationTime &&
                    booking.Item1 > simulationTime.AddMinutes(-1 * simulationSettings.generalSettings.tickLength)) {
                    RequestedBooking rb = booking.Item2;
                    int id = bookingManager.createRequestedBooking(rb.email, rb.location, rb.car, rb.startSoC, rb.targetSoC, rb.timePeriods,
                        rb.priority);
                    try {
                        rb = (RequestedBooking) bookingManager.getBookingById(id);
                    }
                    catch (BookingNotFoundException) {
                        continue;
                    }
                    logger.addRequestedAdHoc(id, rb);
                    BookingDistributor.distributeAdHoc(id, rb, infrastructureManager.getInfrastructureByLocation(SIMULATION_LOCATION_NAME),
                        bookingManager, SIMULATION_LOCATION_NAME);
                }
            }


            int hourOfDistribution = infrastructureManager.getInfrastructureByLocation(SIMULATION_LOCATION_NAME)
                .infrastructureSettings.bookingDistributorActivationTime.Hours;
            // distributor
            if (simulationTime.Hour >= hourOfDistribution
                && (simulationTime.AddMinutes(-1 * simulationSettings.generalSettings.tickLength).Hour < hourOfDistribution)
                || simulationTime.AddMinutes(-1 * simulationSettings.generalSettings.tickLength).Date < simulationTime.Date) {
                BookingDistributor.distribute(bookingManager, SIMULATION_LOCATION_NAME,
                    infrastructureManager.getInfrastructureByLocation(SIMULATION_LOCATION_NAME));
            }
            //add requestedBookings
            while (true) {
                if (index >= upcomingRequestedBookings.Count) {
                    break;
                }
                if (upcomingRequestedBookings[index].Item1 - simulationTime > TimeSpan.Zero) {
                    break;
                }
                RequestedBooking rb = upcomingRequestedBookings[index].Item2;
                bookingManager.createRequestedBooking(rb.email, rb.location, rb.car, rb.startSoC, rb.targetSoC, rb.timePeriods, rb.priority);
                index++;
            }
            // confirm start,end of approved bookings

            var approvedBookings = bookingManager.getAllApprovedBookings();

            foreach (var approvedBooking in approvedBookings.Select(pair =>
                new KeyValuePair<int, ApprovedBooking>(pair.Key, (ApprovedBooking) pair.Value))) {
                if (approvedBooking.Value.timePeriod.startTime - simulationTime <= TimeSpan.Zero
                    && (approvedBooking.Value.timePeriod.startTime) -
                    (simulationTime - TimeSpan.FromMinutes(simulationSettings.generalSettings.tickLength)) > TimeSpan.Zero) {
                    bookingManager.confirmChargeBegin(approvedBooking.Key);
                }
                if (approvedBooking.Value.timePeriod.endTime - simulationTime <= TimeSpan.Zero
                    && (approvedBooking.Value.timePeriod.endTime) -
                    (simulationTime - TimeSpan.FromMinutes(simulationSettings.generalSettings.tickLength)) > TimeSpan.Zero) {
                    bookingManager.confirmChargeEnd(approvedBooking.Key);
                }
            }

            logger.log(infrastructureManager, bookingManager, simulationTime);
        }

        private static int getNextId() {
            return ++idCounter;
        }
    }
}