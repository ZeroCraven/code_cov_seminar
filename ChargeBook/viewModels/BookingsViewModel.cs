using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using chargebook.models;
using ChargeBook.models.booking;

namespace chargebook.viewModels {
    public class BookingsViewModel {

        public int id;
        public string status;
        public IList<TimePeriod> timePeriods;
        public string user;
        public string car;
        public string location;
        public string fullChargePointString;
        public int startSoC;
        public int targetSoC;
        public string connectorType;

        public static BookingsViewModel fromBooking(int id, Booking booking) {
            var fullChargePointString = "";
            var status = "";
            IList<TimePeriod> timePeriods = null;
            var connectorType = "";

            if (booking is ApprovedBooking approvedBooking) {
                fullChargePointString = approvedBooking.chargeGroupName + "-" + approvedBooking.chargeStationName + "-" +
                                        approvedBooking.chargePointIndex;
                status = typeof(ApprovedBookingStatus).GetField(approvedBooking.status.ToString())!.GetCustomAttribute<DisplayAttribute>()!.Name;
                timePeriods = new List<TimePeriod>() {
                    approvedBooking.timePeriod
                };
                connectorType = typeof(ConnectorType).GetField(approvedBooking.connectorType.ToString())!.GetCustomAttribute<DisplayAttribute>()!
                    .Name;
            } else if (booking is RequestedBooking requestedBooking) {
                status = requestedBooking.denied ? "abgelehnt" : "angefordert";
                timePeriods = requestedBooking.timePeriods;
            }

            return new BookingsViewModel() {
                id = id,
                user = booking.email,
                car = booking.car.name,
                location = booking.location,
                startSoC = (int) Math.Round(booking.startSoC * 100),
                targetSoC = (int) Math.Round(booking.targetSoC * 100),
                fullChargePointString = fullChargePointString,
                status = status,
                timePeriods = timePeriods,
                connectorType = connectorType
            };
        }
    }
}