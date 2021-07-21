using System;
using System.Collections.Generic;
using System.Reflection;
using chargebook.models;
using ChargeBook.models.booking;
using chargebook.models.infrastructure;
using ChargeBook.models.simulation.simulationLog;
using ChargeBook.models.simulation.simulationLog.snapshot;
using NUnit.Framework;

namespace UnitTests {
    public class SimulationLogTest {

        private SimulationLog log;
        
        [SetUp]
        public void SetUp() {
            Dictionary<string, ChargeGroup> chargeGroups = new Dictionary<string, ChargeGroup>();
            chargeGroups.Add("group1",new ChargeGroup() {
                chargeStations = new Dictionary<string, ChargeStationType>(),
                maxChargePower = 100,
            });
            Infrastructure infrastructure = new Infrastructure();
            infrastructure.chargeGroups = chargeGroups;
            log = new SimulationLog(infrastructure);
            
            var simulationBookingField = log.GetType().GetField("simulationBookings",
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            var simulationBookings = simulationBookingField.GetValue(log) as Dictionary<int, Booking>;
            simulationBookings.Add(100,new RequestedBooking() {
                car   = new Car(),
                denied = true,
                email = "test"
            });
            
            var snapshots = new List<SimulationSnapshot>();
            snapshots.Add(new SimulationSnapshot() {
                occupiedChargePoints = 3,
                usedPower = 100,
            });
            log.snapshots = snapshots;
            
            
        }

        [Test]
        public void initiateSnapshotTest() {
            Type t = typeof(SimulationLog);
            MethodInfo info = t.GetMethod("initiateSnapshot", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null) {
                info.Invoke(log, new object[] { });
            }
            var snapShotField = log.GetType().GetField("snapshot",
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            var snapshot = snapShotField.GetValue(log) as SimulationSnapshot;
            Assert.IsTrue(snapshot.chargeGroups.ContainsKey("group1"));

        }

        [Test]
        public void endLogCopyBookingsToViewBookingsTest() {
            log.endLog();
            Assert.AreEqual(1,log.viewSimulationBookings.Count);
        }

        [Test]
        public void endLogCreateStatisticsToViewBookingsTest() {
            log.endLog();
            Assert.AreEqual(100, log.statistics.peakWorkload);
            Assert.AreEqual(3, log.statistics.peakOccupancy);
        }
    }
}