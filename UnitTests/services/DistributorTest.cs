using System;
using System.Collections.Generic;
using ChargeBook.data.booking;
using chargebook.models;
using ChargeBook.models.booking;
using chargebook.models.infrastructure;
using NUnit.Framework;

namespace UnitTests.services {
    public class DistributorTest {
        private Infrastructure infrastructure;
        
        [SetUp]
        public void SetUp() {
            Dictionary<string, ChargeGroup> chargeGroups = new Dictionary<string, ChargeGroup>();
            chargeGroups.Add("group1",new ChargeGroup() {
                chargeStations = new Dictionary<string, ChargeStationType>(),
                maxChargePower = 100,
            });
            infrastructure = new Infrastructure();
            infrastructure.infrastructureSettings = new InfrastructureSettings() {
                beginBuffer = 15,
                endBuffer = 15,
            };
            infrastructure.chargeGroups = chargeGroups;
        }

        [Test]
        public void doesAnotherBookingCollideTrue() {
            var list = new List<KeyValuePair<int, ApprovedBooking>>();
            list.Add(new KeyValuePair<int, ApprovedBooking>(1,new ApprovedBooking() {
                chargeGroupName = "a",
                chargeStationName = "1",
                chargePointIndex = 0,
                timePeriod = new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2))
            }));
            Assert.IsTrue(BookingDistributor.doesAnotherBookingCollide(list,infrastructure,"a","1",0, 
                new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2))));
            
        }
        
        [Test]
        public void doesAnotherBookingCollideFalse() {
            var list = new List<KeyValuePair<int, ApprovedBooking>>();
            list.Add(new KeyValuePair<int, ApprovedBooking>(1,new ApprovedBooking() {
                chargeGroupName = "a",
                chargeStationName = "1",
                chargePointIndex = 0,
                timePeriod = new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2))
            }));
            Assert.IsFalse(BookingDistributor.doesAnotherBookingCollide(list,infrastructure,"b","1",0, 
                new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2))));
            
        }

        [Test]
        public void doesNotViolateConnectorTypeReserveFalse() {
            var list = new List<KeyValuePair<int, ApprovedBooking>>();
            list.Add(new KeyValuePair<int, ApprovedBooking>(1,new ApprovedBooking() {
                chargeGroupName = "a",
                chargeStationName = "1",
                chargePointIndex = 0,
                timePeriod = new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2)),
                connectorType = ConnectorType.CCS
            }));
            Assert.IsFalse(BookingDistributor.doesNotViolateConnectorTypeReserve(ConnectorType.CCS, list, infrastructure,
                new TimePeriod(new DateTime(2000, 10, 1), new DateTime(2000, 10, 2))));
        }
        
        [Test]
        public void doesNotViolateConnectorTypeReserveTrue() {
            var list = new List<KeyValuePair<int, ApprovedBooking>>();
            list.Add(new KeyValuePair<int, ApprovedBooking>(1,new ApprovedBooking() {
                chargeGroupName = "a",
                chargeStationName = "1",
                chargePointIndex = 0,
                timePeriod = new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2)),
                connectorType = ConnectorType.CCS
            }));
            Assert.IsTrue(BookingDistributor.doesNotViolateConnectorTypeReserve(ConnectorType.SCHUKO, list, infrastructure,
                new TimePeriod(new DateTime(2000, 10, 1), new DateTime(2000, 10, 2))));
        }

        [Test]
        public void isChargeGroupMaxChargePowerNeverExceededTrue() {
            var list = new List<KeyValuePair<int, ApprovedBooking>>();
            list.Add(new KeyValuePair<int, ApprovedBooking>(1,new ApprovedBooking() {
                chargeGroupName = "a",
                chargeStationName = "1",
                chargePointIndex = 0,
                timePeriod = new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2)),
                connectorType = ConnectorType.CCS
            }));

            Assert.IsTrue(BookingDistributor.isChargeGroupMaxChargePowerNeverExceeded(new TimePeriod(DateTime.Now, DateTime.Now.AddHours(1)), list, infrastructure,
                50,
                "group1", ConnectorType.CCS));
        }
        
        [Test]
        public void isChargeGroupMaxChargePowerNeverExceededFalse() {
            var list = new List<KeyValuePair<int, ApprovedBooking>>();
            list.Add(new KeyValuePair<int, ApprovedBooking>(1,new ApprovedBooking() {
                chargeGroupName = "a",
                chargeStationName = "1",
                chargePointIndex = 0,
                timePeriod = new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2)),
                connectorType = ConnectorType.CCS
            }));

            Assert.IsFalse(BookingDistributor.isChargeGroupMaxChargePowerNeverExceeded(new TimePeriod(DateTime.Now, DateTime.Now.AddHours(1)), list, infrastructure,
                200,
                "group1", ConnectorType.CCS));
        }

        [Test]
        public void tryAllPossibleTimePeriodsTest() {
            var list = new List<TimePeriod>();
            list.Add(new TimePeriod(DateTime.Now, DateTime.Now.AddHours(4)));
            var list1 = new List<KeyValuePair<int, ApprovedBooking>>();
            list1.Add(new KeyValuePair<int, ApprovedBooking>(1,new ApprovedBooking() {
                chargeGroupName = "a",
                chargeStationName = "1",
                chargePointIndex = 0,
                timePeriod = new TimePeriod(new DateTime(2000,10,1),new DateTime(2000,10,2)),
                connectorType = ConnectorType.CCS
            }));
            Assert.IsNotNull(BookingDistributor.tryAllPossibleTimePeriods(TimeSpan.FromHours(1), list, 50, list1, "group1", "1", 0,
                infrastructure, ConnectorType.CCS, true));
        }
    }
}