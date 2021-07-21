using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using chargebook.data.infrastructure;
using chargebook.models;
using ChargeBook.models.booking;
using chargebook.models.infrastructure;
using ChargeBook.models.simulation;
using ChargeBook.models.simulation.settings;
using chargebook.viewModels.simulationViewModels;
using NUnit.Framework;

namespace UnitTests {
    public class SimulationTest {

        private Simulation simulation;

        [SetUp]
        public void SetUp() {
            simulation = new Simulation(new ReadOnlyDictionary<string, int>(new Dictionary<string, int>()),
                new InfrastructureManager(new Dictionary<string, ChargeStationType>()), "testCreator");
        }

        [Test]
        public void testSetGeneralSettings() {
            SimulationGeneralSettingsViewModel settings = new SimulationGeneralSettingsViewModel() {
                name = "testName", seed = 420, tickLength = 15, timePeriod = new TimePeriod(DateTime.Now, DateTime.Now),
            };
            simulation.setGeneralSettings(settings);
            Assert.IsFalse(simulation.simulationSettings.generalSettings.isNotFullySet());
        }

        [Test]
        public void testSetBookingGenerationSetting() {
            List<BookingGenerationSetting> list = new List<BookingGenerationSetting>();
            list.Add(new BookingGenerationSetting());
            list.Add(new BookingGenerationSetting());
            simulation.setBookingGenerationSettings(list);
            Assert.AreEqual(2, simulation.simulationSettings.bookingGenerationSettings.Count);
        }

        [Test]
        public void testGenerateRequestedBookingsCountIsRight() {
            List<Tuple<TimeSpan, TimeSpan>> listTimespans = new List<Tuple<TimeSpan, TimeSpan>>();
            listTimespans.Add(new Tuple<TimeSpan, TimeSpan>(TimeSpan.FromHours(1), TimeSpan.FromHours(23)));
            List<BookingGenerationSetting> list = new List<BookingGenerationSetting>();
            list.Add(new BookingGenerationSetting() {
                bookingDays = new List<DayOfWeek>() {
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday,
                    DayOfWeek.Sunday,
                },
                car = new Car(),
                chargedEnergyInPercent = 10,
                priorityRole = "priorität",
                count = 1,
                requestedTimeSpans = listTimespans,
                requestFrequency = 1,
                timeOfRequest = TimeSpan.Zero,
            });
            simulation.setBookingGenerationSettings(list);
            simulation.simulationSettings.generalSettings.seed = 1000;
            simulation.simulationSettings.generalSettings.tickLength = 15;
            simulation.simulationSettings.generalSettings.tickLength = 3000;
            simulation.simulationSettings.generalSettings.timePeriod = new TimePeriod(new DateTime(2000, 10, 1), new DateTime(2000, 10, 10));

            Type t = typeof(Simulation);
            MethodInfo info = t.GetMethod("generateRequestedBookings", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null) {
                info.Invoke(simulation, new object[] { });
            }
            
            Assert.IsTrue(simulation.upcommingAdHocBookings.Count > 0);
        }

        [Test]
        public void testPrePareForStartFalse() {
            Assert.IsFalse(simulation.prepareForStart());
            SimulationGeneralSettingsViewModel settings = new SimulationGeneralSettingsViewModel() {
                name = "testName", seed = 420, tickLength = 15, timePeriod = new TimePeriod(DateTime.Now, DateTime.Now),
            };
            simulation.setGeneralSettings(settings);
            simulation.setBookingGenerationSettings(new List<BookingGenerationSetting>());
            Assert.IsFalse(simulation.prepareForStart());
        }

    }
}