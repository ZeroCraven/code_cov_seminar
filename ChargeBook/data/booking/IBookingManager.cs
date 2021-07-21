using System;
using System.Collections;
using System.Collections.Generic;
using chargebook.models;
using ChargeBook.models.booking;
using ChargeBook.models.booking.bookingExceptions;
using chargebook.models.infrastructure;

namespace ChargeBook.data.booking {
    public interface IBookingManager {

        public IList<KeyValuePair<int, Booking>> getBookingsByUserEmail(string email);
        public IList<KeyValuePair<int, Booking>> getBookingsByLocation(string location);

        public Booking getBookingById(int bookingId);
        public IList<KeyValuePair<int, Booking>> getAllBookings();
        public IList<KeyValuePair<int, Booking>> getAllApprovedBookings();

        public IDictionary<string, IList<KeyValuePair<int, Booking>>> getCurrentBookingsByUser(string email);


        public void createApprovedBooking(string email, string location, Car car, double startSoC, double targetSoC,
            TimePeriod timePeriod,
            string chargeGroupName, string chargeStationName, int chargePointIndex, ApprovedBookingStatus status, ConnectorType usedConnectorType);


        public void createDeniedRequestedBooking(string email, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods);

        public int createRequestedBooking(int id, string email, bool denied, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods, int priority);

        public int createRequestedBooking(string email, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods, int priority);

        public void deleteBooking(int bookingId);
        public void deleteBookingsFromLocation(string location);
        public void deleteBookingsFromChargeGroup(string location, string chargeGroup);
        public void deleteBookingsFromChargeStation(string location, string chargeGroup, string chargeStation);
        public void deleteBookingsFromUser(string email);


        public void editApprovedBooking(int bookingId, ApprovedBookingStatus status, ConnectorType usedConnectorType, string email = null,
            string location = null, Car car = null,
            TimePeriod timePeriod = null, string chargeGroupName = null, string chargeStationName = null, int chargePointIndex = -1,
            double targetSoC = -1, double startSoC = -1);

        public void editRequestedBooking(int bookingId, bool denied, int priority, string location = null, Car car = null,
            double startSoC = -1,
            double targetSoC = -1, List<TimePeriod> timePeriods = null, string email = null);

        public void approveRequestedBookingLock(int bookingId, TimePeriod timePeriod, string chargeGroupName, string chargeStationName
            , int chargePointIndex, ConnectorType usedConnectorType);


        public void approveRequestedBooking(int bookingId, TimePeriod timePeriod, string chargeGroupName, string chargeStationName
            , int chargePointIndex, ConnectorType usedConnectorType);


        public void confirmChargeBegin(int bookingId);


        public void confirmChargeEnd(int bookingId);

        public bool isUserEntitledToUseBookingId(int bookingId, string email);

        public IList<KeyValuePair<int, ApprovedBooking>> getToConfirmChargeBeginApprovedBookings();
        public IList<KeyValuePair<int, ApprovedBooking>> getEndedToConfirmChargeEndApprovedBookings();

        public void denyBooking(int bookingId);

        public IDictionary<string, IList<KeyValuePair<int, Booking>>> getCurrentBookings();

        public void expireBooking(int id);
    }
}