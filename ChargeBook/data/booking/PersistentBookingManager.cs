using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using chargebook.data;
using chargebook.data.user;
using chargebook.models;
using ChargeBook.models.booking;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;

namespace ChargeBook.data.booking {
    public class PersistentBookingManager : NonPersistentBookingManager, IDisposable {
        private readonly ManagerPersistor persistor;

        public PersistentBookingManager(string pathToBookingsFile) : base(readInitialBookings(pathToBookingsFile)) {
            var self = this;
            persistor = new ManagerPersistor(() => {
                string jsonString;
                lock (self) {
                    jsonString = JsonConvert.SerializeObject(bookings, Formatting.Indented, new JsonSerializerSettings() {
                        TypeNameHandling = TypeNameHandling.Auto,
                    });
                }
                File.WriteAllText(pathToBookingsFile, jsonString, Encoding.UTF8);
            });
        }

        private static Dictionary<int, Booking> readInitialBookings(string path) {
            try {
                return JsonConvert.DeserializeObject<Dictionary<int, Booking>>(File.ReadAllText(path, Encoding.UTF8), new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto,
                });
            }
            catch (FileNotFoundException) {
                return new Dictionary<int, Booking>();
            }
        }

        public void Dispose() {
            persistor.Dispose();
        }

        public override void createApprovedBooking(string email, string location, Car car, double startSoC,
            double targetSoC, TimePeriod timePeriod, string chargeGroupName,
            string chargeStationName, int chargePointIndex, ApprovedBookingStatus status, ConnectorType usedConnectorType) {
            base.createApprovedBooking(email, location, car, startSoC, targetSoC, timePeriod, chargeGroupName, chargeStationName,
                chargePointIndex, status, usedConnectorType);
            persistor.notifyChange();
        }

        public override void approveRequestedBooking(int bookingId, TimePeriod timePeriod, string chargeGroupName
            , string chargeStationName, int chargePointIndex, ConnectorType usedConnectorType) {
            base.approveRequestedBooking(bookingId, timePeriod, chargeGroupName, chargeStationName, chargePointIndex, usedConnectorType);
            persistor.notifyChange();
        }

        public override void confirmChargeBegin(int bookingId) {
            base.confirmChargeBegin(bookingId);
            persistor.notifyChange();
        }

        public override void confirmChargeEnd(int bookingId) {
            base.confirmChargeEnd(bookingId);
            persistor.notifyChange();
        }

        public override int createRequestedBooking(string email, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods, int priority) {
            int result = base.createRequestedBooking(email, location, car, startSoC, targetSoC, timePeriods, priority);
            persistor.notifyChange();
            return result;
        }

        public override void editApprovedBooking(int bookingId, ApprovedBookingStatus status, ConnectorType usedConnectorType, string email = null,
            string location = null, Car car = null, TimePeriod timePeriod = null,
            string chargeGroupName = null, string chargeStationName = null, int chargePointIndex = -1, double targetSoC = -1, double startSoC = -1) {
            base.editApprovedBooking(bookingId, status, usedConnectorType, email, location, car, timePeriod, chargeGroupName, chargeStationName,
                chargePointIndex, targetSoC, startSoC);
            persistor.notifyChange();
        }

        public override void editRequestedBooking(int bookingId, bool denied, int priority, string location = null, Car car = null,
            double startSoC = -1, double targetSoC = -1, List<TimePeriod> timePeriods = null
            , string email = null) {
            base.editRequestedBooking(bookingId, denied, priority, location, car, startSoC, targetSoC, timePeriods, email);
            persistor.notifyChange();
        }

        public override void expireBooking(int id) {
            base.expireBooking(id);
            persistor.notifyChange();
        }

        public override void createDeniedRequestedBooking(string email, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods) {
            base.createDeniedRequestedBooking(email, location, car, startSoC, targetSoC, timePeriods);
            persistor.notifyChange();
        }

        public override int createRequestedBooking(int id, string email, bool denied, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods, int priority) {
            int result = base.createRequestedBooking(id, email, denied, location, car, startSoC, targetSoC, timePeriods, priority);
            persistor.notifyChange();
            return result;
        }

        protected override void unsynchronizedDelete(int bookingId) {
            base.unsynchronizedDelete(bookingId);
            persistor.notifyChange();
        }
    }
}