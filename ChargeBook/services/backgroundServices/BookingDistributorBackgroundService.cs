using System;
using System.Threading;
using System.Threading.Tasks;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using chargebook.models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders.Testing;

namespace chargebook.BackgroundServices {
    public class BookingDistributorBackgroundService : IHostedService, IDisposable {
        private readonly IInfrastructureManager infrastructureManager;
        private readonly IBookingManager bookingManager;

        private Timer timer;

        public BookingDistributorBackgroundService(IInfrastructureManager infrastructureManager, IBookingManager bookingManager) {
            this.infrastructureManager = infrastructureManager;
            this.bookingManager = bookingManager;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            timer = new Timer((state) => {
                foreach (var locationName in infrastructureManager.getInfrastructureToDistribute())
                    BookingDistributor.distribute(bookingManager, locationName,
                        infrastructureManager.getInfrastructureByLocation(locationName));
            }, null, (DateTime.Today + TimeSpan.FromHours(DateTime.Now.Hour + 1)) - DateTime.Now, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose() {
            timer.Dispose();
        }
    }
}