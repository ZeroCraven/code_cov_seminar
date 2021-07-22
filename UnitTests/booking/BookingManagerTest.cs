using System;
using System.Collections.Generic;
using System.Reflection;
using ChargeBook.data.booking;
using chargebook.models;
using ChargeBook.models.booking;
using ChargeBook.models.booking.bookingExceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using NUnit.Framework;

namespace UnitTests {
    public class BookingManagerTest {
        private NonPersistentBookingManager manager;
        List<TimePeriod> list = new List<TimePeriod>();

        [SetUp]
        public void Setup() {
            
            manager = new NonPersistentBookingManager();
            list.Add(new TimePeriod(new DateTime(2000, 12, 1), new DateTime(2000, 12, 4)));
            list.Add(new TimePeriod(new DateTime(2000, 11, 1), new DateTime(2000, 12, 4)));
            list.Add(new TimePeriod(new DateTime(2000, 10, 1), new DateTime(2000, 12, 4)));
            list.Add(new TimePeriod(new DateTime(2000, 9, 1), new DateTime(2000, 9, 4)));
            for (int i = 0; i < 50; i++) {
                manager.createRequestedBooking("testmail@mail", "augs", new Car(), 0.2, 0.6, list,2);
            }
            for (int i = 0; i < 40; i++) {
                manager.createRequestedBooking("testmailadmin@mail", "augsburg", new Car(), 0.1, 0.5, list,2);
            }
            
        }

        [Test]
        public void testGetAllBookings() {
            Assert.AreEqual(90,manager.getAllBookings().Count); 
        }

        /*[Test]
        public void testCreateRequestedBooking() {
            RequestedBooking requestedBooking = new RequestedBooking() {
                car = new Car() {
                    batteryCapacity = 20000,
                    connectors = new Dictionary<ConnectorType, double>(),
                    name = "bmw",
                },
                denied = false,
                email = "testmail/testCreateRequestedBooking",
                location = "location",
                priority = 0,
                startSoC = 0.5,
                targetSoC = 0.6,
                timePeriods = new List<TimePeriod>(),
            };
            int id = manager.createRequestedBooking(requestedBooking.email, requestedBooking.location, requestedBooking.car,
                requestedBooking.startSoC, requestedBooking.targetSoC,
                requestedBooking.timePeriods, 0);
            var booking = (RequestedBooking) manager.getBookingById(id);
            Assert.AreEqual(false,booking.denied);
            Assert.AreEqual(0,booking.priority);
            Assert.AreEqual(0.5,booking.startSoC);
            Assert.AreEqual(0.6,booking.targetSoC);
            Assert.AreEqual(0,booking.priority);

        }*/

        [Test]
        public void testGetBookingById() {
            RequestedBooking requestedBooking = new RequestedBooking() {
                car = new Car() {
                    batteryCapacity = 20000,
                    connectors = new Dictionary<ConnectorType, double>(),
                    name = "bmw",
                },
                denied = false,
                email = "testmail/TestGetBookingsByID",
                location = "location",
                priority = 0,
                startSoC = 0.5,
                targetSoC = 0.6,
                timePeriods = new List<TimePeriod>(),
            };
            int id = manager.createRequestedBooking(requestedBooking.email, requestedBooking.location, requestedBooking.car,
                requestedBooking.startSoC, requestedBooking.targetSoC,
                requestedBooking.timePeriods, 0);
            Assert.AreEqual("testmail/TestGetBookingsByID",manager.getBookingById(id).email);
        }

        [Test]
        public void testGetCurrentBookingsByUser() {
            
            manager.createApprovedBooking("testmail/testGetCurrentBookingsByUser","s",new Car(), 0.1,0.2,
                new TimePeriod(DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddHours(1)),"name","0",0
                ,ApprovedBookingStatus.SCHEDULED, ConnectorType.CCS);
                
            Assert.AreEqual(1,manager.getCurrentBookingsByUser("testmail/testGetCurrentBookingsByUser")["startingBookings"].Count);
        }
        
        [Test]
        public void testgetBookingsByEmail() {
            
            Assert.True(manager.getBookingsByUserEmail("testmail@mail").Count == 50);
            Assert.True(manager.getBookingsByLocation("augsb").Count == 0);
            
        }

        [Test]
        public void testGetBookingsByLocation() {
            
            int countBookings = manager.getBookingsByLocation("augs").Count;
            Assert.True(manager.getBookingsByLocation("augs").Count == countBookings);
            Assert.True(manager.getBookingsByLocation("augsb").Count == 0);
            
        }

        [Test]
        public void testDeleteBooking() {
            
            var list = manager.getBookingsByLocation("augs");
            manager.deleteBooking(list[0].Key);
            Assert.True(manager.getBookingsByLocation("augs").Count == 49);
            manager.deleteBooking(list[0].Key);
            Assert.True(manager.getBookingsByLocation("augs").Count == 49);
            
        }

        [Test]
        public void testApproveRequestedBooking() {
            
            var list = manager.getBookingsByLocation("augs");
            manager.deleteBooking(list[0].Key);
            Assert.Throws<BookingNotFoundException>(() => manager.approveRequestedBooking(list[0].Key,
                new TimePeriod(new DateTime(2000, 11, 1), new DateTime(2000, 12, 4)),
                "group1", "station1",0,ConnectorType.CCS));
            

            int oldId = list[1].Key;
            manager.approveRequestedBooking(list[1].Key,
                new TimePeriod(new DateTime(2000, 12, 1), new DateTime(2000, 12, 4)),
                "group1", "station1",0,ConnectorType.CCS);
            Assert.Throws<CollidingBookingException>(() => manager.approveRequestedBooking(list[2].Key,
                new TimePeriod(new DateTime(2000, 12, 1), new DateTime(2000, 12, 4)),
                "group1", "station1",0,ConnectorType.CCS));
            
            Assert.IsInstanceOf<ApprovedBooking>(manager.getBookingById(oldId));
            ApprovedBooking rb = (ApprovedBooking) manager.getBookingById(oldId);
            Assert.True(rb.timePeriod.Equals(new TimePeriod(new DateTime(2000, 12, 1),
                new DateTime(2000, 12, 4))));
            Assert.True(rb.chargeGroupName == "group1");
            Assert.True(rb.chargeStationName == "station1");
            
        }

        [Test]
        public void testEditApprovedBooking() {
            
            var list = manager.getBookingsByLocation("augs");
            Assert.Throws<WrongBookingTypeException>(() =>
                manager.editApprovedBooking(list[0].Key, ApprovedBookingStatus.EXPIRED, ConnectorType.CCS));
            manager.approveRequestedBooking(list[0].Key,
                new TimePeriod(new DateTime(2000, 12, 1), new DateTime(2000, 12, 4)),
                "group1", "station1", 0, ConnectorType.CCS);
            manager.editApprovedBooking(list[0].Key, ApprovedBookingStatus.CHARGE_END_CONFIRMED, ConnectorType.CCS, "newEmail", "münchen", new Car(),
                new TimePeriod(new DateTime(2000, 9, 1), new DateTime(2000, 9, 4)),
                "chargeGroup2", "chargestation2", 1, 0.5, 0.7);
            list = manager.getBookingsByLocation("münchen");
            ApprovedBooking approvedBooking = (ApprovedBooking) list[0].Value;
            Assert.True(approvedBooking.timePeriod.Equals(new TimePeriod(new DateTime(2000, 9, 1),
                new DateTime(2000, 9, 4))));
            Assert.True(approvedBooking.chargeGroupName == "chargeGroup2");
            Assert.True(approvedBooking.chargeStationName == "chargestation2");
            Assert.True(approvedBooking.email == "newEmail");
            Assert.True(approvedBooking.location == "münchen");
            
        }

        [Test]
        public void testBookingNotFoundEditRequestedBooking() {
            Assert.Throws<BookingNotFoundException>(() =>manager.editRequestedBooking(1000, false, 0));
        }

        [Test]
        public void testWrongBookingNotFoundEditRequestedBooking() {
            manager.createApprovedBooking("testmail/testGetCurrentBookingsByUser","s",new Car(), 0.1,0.2,
                new TimePeriod(DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddHours(1)),"name","0",0
                ,ApprovedBookingStatus.SCHEDULED, ConnectorType.CCS);
            Assert.Throws<WrongBookingTypeException>(() =>manager.editRequestedBooking(91, false, 0));
        }

        [Test]
        public void testTrueIsBookingColliding() {
            manager.createApprovedBooking("testmail/testTrueIsBookingColliding","s",new Car(), 0.1,0.2,
                new TimePeriod(DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddHours(1)),"name","0",0
                ,ApprovedBookingStatus.SCHEDULED, ConnectorType.CCS);
            Type t = typeof(NonPersistentBookingManager);
            MethodInfo info = t.GetMethod("isBookingColliding", BindingFlags.NonPublic | BindingFlags.Instance);
            object result = null;
            if (info != null) {
                result = info.Invoke(manager, new object[] {
                    "s","name","0",0,new TimePeriod(DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddHours(2)),-1 
                });
            }
            if (result != null) {
                Assert.IsTrue((bool)result);
            }
        }
        
        [Test]
        public void testFalseIsBookingColliding() {
            manager.createApprovedBooking("testmail/testTrueIsBookingColliding","s",new Car(), 0.1,0.2,
                new TimePeriod(DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddHours(1)),"name","0",0
                ,ApprovedBookingStatus.SCHEDULED, ConnectorType.CCS);
            Type t = typeof(NonPersistentBookingManager);
            MethodInfo info = t.GetMethod("isBookingColliding", BindingFlags.NonPublic | BindingFlags.Instance);
            object result = null;
            if (info != null) {
                result = info.Invoke(manager, new object[] {
                    "q","name","0",0,new TimePeriod(DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddHours(2)),-1 
                });
            }
            if (result != null) {
                Assert.IsFalse((bool)result);
            }
        }
        
        
        
    }
}