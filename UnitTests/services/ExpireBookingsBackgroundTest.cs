using System;
using System.Collections.Generic;
using System.Linq;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using chargebook.models;
using ChargeBook.models.booking;
using chargebook.models.infrastructure;
using ChargeBook.services.backgroundServices;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace UnitTests.services {
    public class ExpireBookingsBackgroundTest {
        private NonPersistentBookingManager bookingManager;
        private InfrastructureManager infrastructureManager;
        
        [SetUp]
        public void setUp() {
            
            infrastructureManager = new InfrastructureManager(new Dictionary<string, ChargeStationType>());
            infrastructureManager.createLocation("Augsburg",TimeZoneInfo.Utc);
            infrastructureManager.getInfrastructureByLocation("Augsburg").infrastructureSettings.beginBuffer = 15;
            
            bookingManager = new NonPersistentBookingManager();
            bookingManager.createApprovedBooking("email", "Augsburg",new Car(),0.1,0.2, 
                new TimePeriod(DateTime.UtcNow.AddHours(-2),DateTime.UtcNow.AddHours(3)) ,"grupp1","station2",0,
                ApprovedBookingStatus.SCHEDULED, ConnectorType.CCS);
            bookingManager.createApprovedBooking("email", "Augsburg",new Car(),0.1,0.2, 
                new TimePeriod(DateTime.UtcNow.AddHours(+2),DateTime.UtcNow.AddHours(3)) ,"grupp2","station2",0,
                ApprovedBookingStatus.SCHEDULED,ConnectorType.CCS);
                
        }
        [Test]
        public void expireTest() {
            
            ExpireBookingsBackgroundService expireService = new ExpireBookingsBackgroundService(bookingManager,infrastructureManager);
            expireService.expire(null);
            var newBookings = bookingManager.getBookingsByUserEmail("email");
            Assert.True(((ApprovedBooking)newBookings[0].Value).status == ApprovedBookingStatus.EXPIRED);
            Assert.False(((ApprovedBooking)newBookings[1].Value).status == ApprovedBookingStatus.EXPIRED);
        }
    }
}