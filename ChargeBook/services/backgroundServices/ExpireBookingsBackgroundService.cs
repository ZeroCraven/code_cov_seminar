using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using ChargeBook.models.booking;
using Microsoft.Extensions.Hosting;

namespace ChargeBook.services.backgroundServices {
    public class ExpireBookingsBackgroundService : IHostedService, IDisposable {
        private Timer timer;
        private readonly IBookingManager bookingManager;
        private readonly IInfrastructureManager infrastructureManager;

        public ExpireBookingsBackgroundService(IBookingManager bookingManager, IInfrastructureManager infrastructureManager) {
            this.bookingManager = bookingManager;
            this.infrastructureManager = infrastructureManager;
        }

        public Task StartAsync(CancellationToken stoppingToken) {
            timer = new Timer(expire, null,
                DateTime.Today.AddHours(DateTime.Now.Hour).AddMinutes((DateTime.Now.Minute / 15 + 1) * 15 + 0.5) - DateTime.Now,
                TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        public void expire(object state) {
            try {
                bookingManager.getAllBookings()
                    .Where(x => x.Value is ApprovedBooking)
                    .Select(x => new KeyValuePair<int, ApprovedBooking>(x.Key, x.Value as ApprovedBooking))
                    .Where(x => x.Value.status == ApprovedBookingStatus.SCHEDULED)
                    .Where(x => {
                        var buffer = infrastructureManager.getInfrastructureByLocation(x.Value.location).infrastructureSettings.beginBuffer;
                        return DateTime.UtcNow - x.Value.timePeriod.startTime > TimeSpan.FromMinutes(buffer);
                    }).ToList().ForEach(pair => bookingManager.expireBooking(pair.Key));
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken) {
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose() {
            timer?.Dispose();
        }

    }
}