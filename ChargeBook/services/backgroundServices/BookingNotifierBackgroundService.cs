using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChargeBook.data.booking;
using chargebook.data.user;
using chargebook.models;
using ChargeBook.models.booking;
using chargebook.models.email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChargeBook.services.backgroundServices {
    public class BookingNotifierBackgroundService : IHostedService, IDisposable {

        private Timer timer;
        private readonly IServiceProvider serviceProvider;
        private IUserManager userManager;
        private IBookingManager bookingManager;
        private readonly string bookingStartMessage;
        private readonly string bookingEndMessage;

        public BookingNotifierBackgroundService(IServiceProvider serviceProvider, IUserManager userManager, IBookingManager bookingManager) {
            this.serviceProvider = serviceProvider;
            this.userManager = userManager;
            this.bookingManager = bookingManager;

            bookingStartMessage =
                "This is the text version of the html string.It is sent, if " +
                "displaying the html version fails";
            bookingEndMessage =
                "This is the text version of the html string.It is sent, if " +
                "displaying the html version fails";
        }

        /// <summary>
        /// starts async the backgroundService
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns>Task.CompletedTask</returns>
        public Task StartAsync(CancellationToken stoppingToken) {
            timer = new Timer(notify, null, DateTime.Today.AddHours(DateTime.Now.Hour).AddMinutes((DateTime.Now.Minute / 5 + 1) * 5) - DateTime.Now,
                TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }


        /// <summary>
        /// loops through each booking and notifies his owner, if the booking is
        /// imminent or will end soon
        /// </summary>
        /// <param name="state"></param>
        private void notify(object state) {
            var scope = serviceProvider.CreateScope();
            IViewRenderService renderer;
            IEmailSender emailSender;
            try {
                renderer = scope.ServiceProvider.GetRequiredService<IViewRenderService>();
                emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return;
            }
            bookingManager.getToConfirmChargeBeginApprovedBookings()
                .Where(pair => userManager.isNotificationEnabled(pair.Value.email))
                .Select(x => notifyUserBookingStart(x.Value.email, x.Key, x.Value, renderer, emailSender))
                .ToList()
                .ForEach(task => task.Wait());

            bookingManager.getEndedToConfirmChargeEndApprovedBookings()
                .Where(pair => userManager.isNotificationEnabled(pair.Value.email))
                .Select(x => notifyUserBookingEnd(x.Value.email, x.Key, x.Value, renderer, emailSender))
                .ToList()
                .ForEach(task => task.Wait());
        }

        public string createEmailHtmlString(string viewName, object model, IViewRenderService renderer) {
            try {
                return renderer.renderToStringAsync(viewName, model).Result;
            }
            catch (Exception) {
                return "";
            }
        }

        private Task notifyUserBookingStart(string email, int bookingId, Booking booking, IViewRenderService renderer, IEmailSender emailSender) {
            EmailInformation emailInfo = new EmailInformation() {
                car = booking.car,
                location = booking.location,
                name = userManager.getUsername(email),
                timePeriod = ((ApprovedBooking) booking).timePeriod,
                chargeStation = ((ApprovedBooking) booking).chargeStationName
            };
            string htmlMessage = createEmailHtmlString("email/bookingStartEmail", emailInfo, renderer);
            string textMessage = bookingStartMessage;
            try {
                return emailSender.sendEmailAsync(email, "Buchungsbeginn", htmlMessage, textMessage, userManager.getUsername(email));
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return Task.CompletedTask;
            }
        }

        private Task notifyUserBookingEnd(string email, int bookingId, Booking booking, IViewRenderService renderer, IEmailSender emailSender) {
            EmailInformation emailInfo = new EmailInformation() {
                car = booking.car,
                location = booking.location,
                name = userManager.getUsername(email),
                timePeriod = ((ApprovedBooking) booking).timePeriod,
                chargeStation = ((ApprovedBooking) booking).chargeStationName
            };
            string htmlMessage = createEmailHtmlString("email/bookingEndEmail", emailInfo, renderer);
            string textMessage = bookingEndMessage;
            try {
                DateTime x = ((ApprovedBooking) booking).timePeriod.startTime;
                return emailSender.sendEmailAsync(email, "Ende ihrer Buchung in " + booking.location + " am " + x.Day + "." + x.Month + ".",
                    htmlMessage,
                    textMessage, userManager.getUsername(email));
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return Task.CompletedTask;
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