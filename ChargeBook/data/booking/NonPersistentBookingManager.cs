using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using chargebook.data.user;
using chargebook.models;
using ChargeBook.models.booking;
using ChargeBook.models.booking.bookingExceptions;

namespace ChargeBook.data.booking {
    public class NonPersistentBookingManager : IBookingManager {
        protected Dictionary<int, Booking> bookings;
        private int idCounter = 0;

        public NonPersistentBookingManager(Dictionary<int, Booking> initialBookings = null) {
            idCounter = 0;
            bookings = new Dictionary<int, Booking>();
            if (initialBookings != null && initialBookings.Count != 0) {
                bookings = initialBookings;
                idCounter = initialBookings.Keys.Max();
            }
        }

        public IList<KeyValuePair<int, Booking>> getAllBookings() {
            List<KeyValuePair<int, Booking>> list;
            lock (this) {
                list = new List<KeyValuePair<int, Booking>>(bookings);
            }
            return sortList(list);
        }

        public Booking getBookingById(int bookingId) {
            Booking booking;
            lock (this) {
                if (!bookings.ContainsKey(bookingId)) {
                    throw new BookingNotFoundException("The bookingId doesnt exists in the system");
                }
                booking = bookings[bookingId];
            }
            return booking;
        }

        public IList<KeyValuePair<int, Booking>> getAllApprovedBookings() {
            List<KeyValuePair<int, Booking>> list;
            lock (this) {
                list = new List<KeyValuePair<int, Booking>>(bookings.Where(pair => pair.Value.GetType() == typeof(ApprovedBooking)));
            }
            return sortList(list);
        }

        public IList<KeyValuePair<int, Booking>> getBookingsByUserEmail(string email) {
            List<KeyValuePair<int, Booking>> list;
            lock (this) {
                list = new List<KeyValuePair<int, Booking>>(bookings.Where(pair => pair.Value.email == email));
            }
            return sortList(list);
        }

        public IList<KeyValuePair<int, Booking>> getBookingsByLocation(string location) {
            List<KeyValuePair<int, Booking>> list;
            lock (this) {
                list = new List<KeyValuePair<int, Booking>>(bookings.Where(pair => pair.Value.location == location));
            }
            return sortList(list);
        }

        public IDictionary<string, IList<KeyValuePair<int, Booking>>> getCurrentBookings() {
            var dictionary = new Dictionary<string, IList<KeyValuePair<int, Booking>>>();
            lock (this) {
                List<KeyValuePair<int, Booking>> bookingList;

                bookingList = new List<KeyValuePair<int, Booking>>(bookings.Where(x => x.Value.GetType() == typeof(ApprovedBooking)));

                List<KeyValuePair<int, Booking>> startingBookings = new List<KeyValuePair<int, Booking>>(bookingList
                    .Where(x => ((ApprovedBooking) x.Value).status == ApprovedBookingStatus.SCHEDULED).Where(x => {
                        var startBookingDateTime = ((ApprovedBooking) x.Value).timePeriod.startTime;
                        var endBookingDateTime = ((ApprovedBooking) x.Value).timePeriod.endTime;
                        return startBookingDateTime <= DateTime.UtcNow && DateTime.UtcNow <= endBookingDateTime;
                    }));
                IList<KeyValuePair<int, Booking>> startingBookingsReadOnly = sortList(startingBookings);
                dictionary.Add("startingBookings", startingBookingsReadOnly);

                List<KeyValuePair<int, Booking>> ongoingBookings =
                    new List<KeyValuePair<int, Booking>>(bookingList
                        .Where(x => ((ApprovedBooking) x.Value).status == ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED));
                IList<KeyValuePair<int, Booking>> ongoingBookingsReadOnly = sortList(ongoingBookings);
                dictionary.Add("ongoingBookings", ongoingBookingsReadOnly);
            }
            return dictionary;
        }

        public IDictionary<string, IList<KeyValuePair<int, Booking>>> getCurrentBookingsByUser(string email) {
            var dictionary = new Dictionary<string, IList<KeyValuePair<int, Booking>>>();
            lock (this) {
                var bookingList =
                    new List<KeyValuePair<int, Booking>>(bookings.Where(x => x.Value.email == email)
                        .Where(x => x.Value.GetType() == typeof(ApprovedBooking)));

                List<KeyValuePair<int, Booking>> startingBookings = new List<KeyValuePair<int, Booking>>(bookingList
                    .Where(x => ((ApprovedBooking) x.Value).status == ApprovedBookingStatus.SCHEDULED).Where(x => {
                        var startBookingDateTime = ((ApprovedBooking) x.Value).timePeriod.startTime;
                        var endBookingDateTime = ((ApprovedBooking) x.Value).timePeriod.endTime;
                        return startBookingDateTime <= DateTime.UtcNow && DateTime.UtcNow <= endBookingDateTime;
                    }));
                IList<KeyValuePair<int, Booking>> startingBookingsReadOnly = sortList(startingBookings);
                dictionary.Add("startingBookings", startingBookingsReadOnly);

                List<KeyValuePair<int, Booking>> ongoingBookings =
                    new List<KeyValuePair<int, Booking>>(bookingList
                        .Where(x => ((ApprovedBooking) x.Value).status == ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED));
                IList<KeyValuePair<int, Booking>> ongoingBookingsReadOnly = sortList(ongoingBookings);
                dictionary.Add("ongoingBookings", ongoingBookingsReadOnly);
            }
            return dictionary;
        }

        public virtual void createDeniedRequestedBooking(string email, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods) {
            var bookingId = ++idCounter;
            timePeriods.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            RequestedBooking newBooking = new RequestedBooking() {
                email = email,
                location = location,
                car = car,
                startSoC = startSoC,
                targetSoC = targetSoC,
                timePeriods = timePeriods,
                denied = true,
            };
            bookings.Add(bookingId, newBooking);
        }

        public virtual int createRequestedBooking(int id, string email, bool denied, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods, int priority) {
            timePeriods.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            RequestedBooking newBooking = new RequestedBooking() {
                email = email,
                location = location,
                car = car,
                startSoC = startSoC,
                targetSoC = targetSoC,
                timePeriods = timePeriods,
                denied = denied,
                priority = priority,
            };
            lock (this) {
                bookings.Add(id, newBooking);
            }
            return id;
        }

        public virtual int createRequestedBooking(string email, string location, Car car, double startSoC, double targetSoC,
            List<TimePeriod> timePeriods, int priority) {
            var bookingId = ++idCounter;
            timePeriods.Sort((x, y) => x.startTime.CompareTo(y.startTime));
            RequestedBooking newBooking = new RequestedBooking() {
                email = email,
                location = location,
                car = car,
                startSoC = startSoC,
                targetSoC = targetSoC,
                timePeriods = timePeriods,
                priority = priority
            };
            lock (this) {
                bookings.Add(bookingId, newBooking);
            }
            return bookingId;
        }

        protected virtual void unsynchronizedDelete(int bookingId) {
            if (bookings.ContainsKey(bookingId)) {
                bookings.Remove(bookingId);
            }
        }

        public virtual void createApprovedBooking(string email, string location, Car car, double startSoC, double targetSoC,
            TimePeriod timePeriod,
            string chargeGroupName,
            string chargeStationName, int chargePointIndex,
            ApprovedBookingStatus status, ConnectorType usedConnectorType) {
            lock (this) {
                var bookingId = ++idCounter;
                if (isBookingColliding(location, chargeGroupName, chargeStationName, chargePointIndex, timePeriod)) {
                    throw new CollidingBookingException();
                }
                ApprovedBooking newBooking = new ApprovedBooking() {
                    email = email,
                    location = location,
                    car = car,
                    startSoC = startSoC,
                    targetSoC = targetSoC,
                    timePeriod = timePeriod,
                    chargeGroupName = chargeGroupName,
                    chargeStationName = chargeStationName,
                    chargePointIndex = chargePointIndex,
                    status = status,
                    connectorType = usedConnectorType
                };
                bookings.Add(bookingId, newBooking);
            }
        }


        public void deleteBooking(int bookingId) {
            lock (this) {
                unsynchronizedDelete(bookingId);
            }
        }

        public void deleteBookingsFromLocation(string location) {
            lock (this) {
                bookings.Where(x => x.Value.location == location)
                    .ToList()
                    .ForEach(x => unsynchronizedDelete(x.Key));
            }
        }

        public void deleteBookingsFromChargeGroup(string location, string chargeGroup) {
            lock (this) {
                foreach (var keyValuePair in bookings.Where(x => x.Value.GetType() == typeof(ApprovedBooking))
                    .Where(x => x.Value.location == location && ((ApprovedBooking) x.Value).chargeGroupName == chargeGroup)) {
                    unsynchronizedDelete(keyValuePair.Key);
                }
            }
        }

        public void deleteBookingsFromChargeStation(string location, string chargeGroup, string chargeStation) {
            lock (this) {
                foreach (var keyValuePair in bookings.Where(x => x.Value.GetType() == typeof(ApprovedBooking))
                    .Where(x => x.Value.location == location && ((ApprovedBooking) x.Value).chargeGroupName == chargeGroup
                                                             && ((ApprovedBooking) x.Value).chargeStationName == chargeStation)) {
                    unsynchronizedDelete(keyValuePair.Key);
                }
            }
        }

        public void deleteBookingsFromUser(string email) {
            lock (this) {
                foreach (var keyValuePair in bookings.Where(x => x.Value.email == email)) {
                    unsynchronizedDelete(keyValuePair.Key);
                }
            }
        }

        public virtual void editApprovedBooking(int bookingId, ApprovedBookingStatus status, ConnectorType usedConnectorType, string email = null,
            string location = null, Car car = null,
            TimePeriod timePeriod = null, string chargeGroupName = null, string chargeStationName = null, int chargePointIndex = -1,
            double targetSoC = -1, double startSoC = -1) {
            lock (this) {
                if (!bookings.ContainsKey(bookingId)) {
                    throw new BookingNotFoundException("The bookingId doesnt exists in the system");
                }
                if (bookings[bookingId].GetType() != typeof(ApprovedBooking)) {
                    throw new WrongBookingTypeException("the bookingId: " + bookingId + " doesnt references an approved booking");
                }

                ApprovedBooking bookingToChange = (ApprovedBooking) bookings[bookingId];

                email ??= bookingToChange.email;
                location ??= bookingToChange.location;
                car ??= bookingToChange.car;
                timePeriod ??= bookingToChange.timePeriod;
                chargeGroupName ??= bookingToChange.chargeGroupName;
                chargeStationName ??= bookingToChange.chargeStationName;
                chargePointIndex = (chargePointIndex < 0) ? bookingToChange.chargePointIndex : chargePointIndex;
                startSoC = (startSoC < 0) ? (bookingToChange.startSoC) : (startSoC);
                targetSoC = (targetSoC < 0) ? (bookingToChange.targetSoC) : (targetSoC);

                if (isBookingColliding(location, chargeGroupName, chargeStationName, chargePointIndex, timePeriod, bookingId)) {
                    throw new CollidingBookingException("the changes to the booking with id " + bookingId + " collides with another " +
                                                        "approved booking");
                }
                bookingToChange.email = email;
                bookingToChange.location = location;
                bookingToChange.car = car;
                bookingToChange.timePeriod = timePeriod;
                bookingToChange.chargeGroupName = chargeGroupName;
                bookingToChange.chargeStationName = chargeStationName;
                bookingToChange.startSoC = startSoC;
                bookingToChange.targetSoC = targetSoC;
                bookingToChange.status = status;
                bookingToChange.chargePointIndex = chargePointIndex;
                bookingToChange.connectorType = usedConnectorType;
            }
        }

        public virtual void editRequestedBooking(int bookingId, bool denied, int priority, string location = null, Car car = null,
            double startSoC = -1,
            double targetSoC = -1, List<TimePeriod> timePeriods = null, string email = null) {
            lock (this) {
                if (!bookings.ContainsKey(bookingId)) {
                    throw new BookingNotFoundException("The bookingId doesnt exists in the system");
                }
                if (bookings[bookingId].GetType() != typeof(RequestedBooking)) {
                    throw new WrongBookingTypeException("the bookingId: " + bookingId + " doesnt references a requested booking");
                }

                RequestedBooking bookingToChange = (RequestedBooking) bookings[bookingId];

                email ??= bookingToChange.email;
                location ??= bookingToChange.location;
                car ??= bookingToChange.car;
                timePeriods ??= bookingToChange.timePeriods;
                startSoC = (startSoC < 0) ? (bookingToChange.startSoC) : (startSoC);
                targetSoC = (targetSoC < 0) ? (bookingToChange.targetSoC) : (targetSoC);


                bookingToChange.email = email;
                bookingToChange.location = location;
                bookingToChange.car = car;
                bookingToChange.timePeriods = timePeriods;
                bookingToChange.startSoC = startSoC;
                bookingToChange.targetSoC = targetSoC;
                bookingToChange.denied = denied;
                bookingToChange.priority = priority;
            }
        }

        public void approveRequestedBookingLock(int bookingId, TimePeriod timePeriod, string chargeGroupName, string chargeStationName
            , int chargePointIndex, ConnectorType usedConnectorType) {
            lock (this) {
                approveRequestedBooking(bookingId, timePeriod, chargeGroupName, chargeStationName, chargePointIndex, usedConnectorType);
            }
        }

        public virtual void approveRequestedBooking(int bookingId, TimePeriod timePeriod, string chargeGroupName, string chargeStationName
            , int chargePointIndex, ConnectorType usedConnectorType) {
            if (!bookings.ContainsKey(bookingId)) {
                throw new BookingNotFoundException("The bookingId doesnt exists in the system");
            }
            if (bookings[bookingId].GetType() != typeof(RequestedBooking)) {
                throw new WrongBookingTypeException("the bookingId: " + bookingId + " doesnt references a requested booking");
            }

            RequestedBooking requestedBookingToApprove = (RequestedBooking) bookings[bookingId];

            if (isBookingColliding(requestedBookingToApprove.location, chargeGroupName, chargeStationName, chargePointIndex, timePeriod)) {
                throw new CollidingBookingException("the changes to the booking with id " + bookingId + " collides with another " +
                                                    "approved booking");
            }

            ApprovedBooking approvedBooking = new ApprovedBooking() {
                car = requestedBookingToApprove.car,
                chargeGroupName = chargeGroupName,
                chargeStationName = chargeStationName,
                email = requestedBookingToApprove.email,
                location = requestedBookingToApprove.location,
                startSoC = requestedBookingToApprove.startSoC,
                status = ApprovedBookingStatus.SCHEDULED,
                targetSoC = requestedBookingToApprove.targetSoC,
                timePeriod = timePeriod,
                chargePointIndex = chargePointIndex,
                connectorType = usedConnectorType,
            };
            deleteBooking(bookingId);
            bookings.Add(bookingId, approvedBooking);
        }

        public virtual void confirmChargeBegin(int bookingId) {
            lock (this) {
                if (!bookings.ContainsKey(bookingId)) {
                    throw new BookingNotFoundException("The bookingId doesnt exists in the system");
                }
                if (bookings[bookingId].GetType() != typeof(ApprovedBooking)) {
                    throw new WrongBookingTypeException("the bookingId: " + bookingId + " doesnt references a requested booking");
                }
                ApprovedBooking approvedBooking = (ApprovedBooking) bookings[bookingId];

                if (approvedBooking.status == ApprovedBookingStatus.SCHEDULED) {
                    approvedBooking.status = ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED;
                }
            }
        }

        public virtual void confirmChargeEnd(int bookingId) {
            lock (this) {
                if (!bookings.ContainsKey(bookingId)) {
                    throw new BookingNotFoundException("The bookingId doesnt exists in the system");
                }
                if (bookings[bookingId].GetType() != typeof(ApprovedBooking)) {
                    throw new WrongBookingTypeException("the bookingId: " + bookingId + " doesnt references a requested booking");
                }
                ApprovedBooking approvedBooking = (ApprovedBooking) bookings[bookingId];

                if (approvedBooking.status == ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED) {
                    approvedBooking.status = ApprovedBookingStatus.CHARGE_END_CONFIRMED;
                }
            }
        }

        public bool isUserEntitledToUseBookingId(int bookingId, string email) {
            Booking var;
            if (bookings.TryGetValue(bookingId, out var)) {
                return var.email == email;
            }
            return false;
        }


        private bool isBookingColliding(string location, string chargeGroupName, string chargeStationName, int chargePointIndex,
            TimePeriod timePeriod, int id = -1) {
            return bookings.Where(x => x.Value.GetType() == typeof(ApprovedBooking))
                .Select(x => new KeyValuePair<int, ApprovedBooking>(x.Key, (ApprovedBooking) x.Value))
                .Where(approvedBooking => approvedBooking.Value.location == location && approvedBooking.Value.chargeGroupName == chargeGroupName &&
                                          approvedBooking.Value.chargeStationName == chargeStationName
                                          && approvedBooking.Value.chargePointIndex == chargePointIndex)
                .Any(approvedBooking =>
                    approvedBooking.Value.timePeriod.startTime.CompareTo(timePeriod.endTime) <= 0 &&
                    approvedBooking.Value.timePeriod.endTime.CompareTo(timePeriod.startTime) >= 0 && approvedBooking.Key != id);
        }

        private IList<KeyValuePair<int, Booking>> sortList(List<KeyValuePair<int, Booking>> list) {
            list.Sort((x, y) => {
                var xDateTime = x.Value.GetType() == typeof(ApprovedBooking)
                    ? ((ApprovedBooking) x.Value).timePeriod.startTime
                    : ((RequestedBooking) x.Value).timePeriods[0].startTime;
                var yDateTime = y.Value.GetType() == typeof(ApprovedBooking)
                    ? ((ApprovedBooking) y.Value).timePeriod.startTime
                    : ((RequestedBooking) y.Value).timePeriods[0].startTime;
                return DateTime.Compare(xDateTime, yDateTime);
            });
            return list.AsReadOnly();
        }

        public IList<KeyValuePair<int, ApprovedBooking>> getToConfirmChargeBeginApprovedBookings() {
            lock (this) {
                return bookings.Where(pair => pair.Value.GetType() == typeof(ApprovedBooking))
                    .Select(pair => new KeyValuePair<int, ApprovedBooking>(pair.Key, pair.Value as ApprovedBooking))
                    .Where(pair => pair.Value.status == ApprovedBookingStatus.SCHEDULED)
                    .Where(pair => (pair.Value.timePeriod.startTime - DateTime.UtcNow).Duration() < TimeSpan.FromMinutes(2))
                    .ToList()
                    .AsReadOnly();
            }
        }

        public IList<KeyValuePair<int, ApprovedBooking>> getEndedToConfirmChargeEndApprovedBookings() {
            lock (this) {
                return bookings.Where(pair => pair.Value.GetType() == typeof(ApprovedBooking))
                    .Select(pair => new KeyValuePair<int, ApprovedBooking>(pair.Key, pair.Value as ApprovedBooking))
                    .Where(pair => pair.Value.status == ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED)
                    .Where(pair => (pair.Value.timePeriod.endTime - DateTime.UtcNow).Duration() < TimeSpan.FromMinutes(2))
                    .ToList()
                    .AsReadOnly();
            }
        }

        public void denyBooking(int bookingId) {
            if (bookings.ContainsKey(bookingId)) {
                ((RequestedBooking) bookings[bookingId]).denied = true;
            }
        }

        public virtual void expireBooking(int id) {
            lock (this) {
                if (!bookings.ContainsKey(id)) {
                    throw new BookingNotFoundException();
                }
                if (bookings[id] is ApprovedBooking approvedBooking) {
                    if (approvedBooking.status != ApprovedBookingStatus.SCHEDULED) {
                        throw new WrongBookingStatusException();
                    }
                    approvedBooking.status = ApprovedBookingStatus.EXPIRED;
                    return;
                }
                throw new WrongBookingTypeException();
            }
        }
    }

    #region BookingExceptions

    public class WrongBookingStatusException : Exception {
        public WrongBookingStatusException() : base() { }
    }

    #endregion

}