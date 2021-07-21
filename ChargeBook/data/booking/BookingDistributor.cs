using System;
using System.Collections.Generic;
using System.Linq;
using chargebook.models;
using ChargeBook.models.booking;
using ChargeBook.models.booking.bookingExceptions;
using chargebook.models.infrastructure;


namespace ChargeBook.data.booking {
    public class BookingDistributor {
        public static void distribute(IBookingManager bookingManager, string locationName, Infrastructure infrastructure) {
            lock (bookingManager) {
                var bookings = bookingManager.getBookingsByLocation(locationName);
                var requestedBookings =
                    bookings.Where(pair => pair.Value.GetType() == typeof(RequestedBooking))
                        .Select(pair => new KeyValuePair<int, RequestedBooking>(pair.Key, pair.Value as RequestedBooking))
                        .Where(pair => pair.Value.denied != true)
                        .OrderByDescending(pair => pair.Value.getRequestedChargeEnergy() + pair.Value.priority);
                var scheduledBookings = getScheduledBookings(locationName, bookingManager);
                foreach (var (id, booking) in requestedBookings) {
                    tryToApproveBooking(id, booking, infrastructure, bookingManager, scheduledBookings, false);
                }
            }
        }

        public static void tryToApproveBooking(int bookingId, RequestedBooking booking, Infrastructure infrastructure, IBookingManager bookingManager,
            IList<KeyValuePair<int, ApprovedBooking>> scheduledBookings, bool ignoreConnectorTypeReserve) {
            var carConnectors = booking.car.connectors.OrderByDescending(x => x.Value);
            foreach (var carConnector in carConnectors) {
                var availableChargePoints =
                    infrastructure.getAvailableChargePointsByConnectorType(carConnector.Key)
                        .OrderByDescending(tuple => Math.Min(tuple.Item4, carConnector.Value));
                foreach (var chargePoint in availableChargePoints) {
                    double chargePower = Math.Min(chargePoint.Item4, carConnector.Value);
                    double chargeTimeInHours = booking.getRequestedChargeEnergy() / chargePower;
                    var chargeTime = ceilToNext15Minutes(chargeTimeInHours);
                    var validTimePeriod = tryAllPossibleTimePeriods(chargeTime, booking.timePeriods, chargePower, scheduledBookings,
                        chargePoint.Item1,
                        chargePoint.Item2, chargePoint.Item3, infrastructure, carConnector.Key, ignoreConnectorTypeReserve);
                    if (validTimePeriod != null) {
                        bookingManager.approveRequestedBooking(bookingId, validTimePeriod, chargePoint.Item1, chargePoint.Item2,
                            chargePoint.Item3, carConnector.Key);
                        scheduledBookings.Add(new KeyValuePair<int, ApprovedBooking>(bookingId,
                            bookingManager.getBookingById(bookingId) as ApprovedBooking));
                        return;
                    }
                }
                bookingManager.denyBooking(bookingId);
            }
        }

        public static bool doesAnotherBookingCollide(IEnumerable<KeyValuePair<int, ApprovedBooking>> approvedBooking,
            Infrastructure infrastructure,
            string chargeGroup, string chargeStation, int chargePoint, TimePeriod timePeriod) {
            return approvedBooking
                .Where(pair => pair.Value.chargeGroupName == chargeGroup)
                .Where(pair => pair.Value.chargeStationName == chargeStation)
                .Where(pair => pair.Value.chargePointIndex == chargePoint)
                .Any(booking =>
                    (booking.Value.timePeriod.endTime + TimeSpan.FromMinutes(infrastructure.infrastructureSettings.endBuffer) >=
                     timePeriod.startTime &&
                     booking.Value.timePeriod.startTime <=
                     timePeriod.endTime + TimeSpan.FromMinutes(infrastructure.infrastructureSettings.endBuffer)));
        }

        public static TimeSpan ceilToNext15Minutes(double hours) {
            return TimeSpan.FromMinutes(((int) TimeSpan.FromHours(hours).TotalMinutes / 15 + 1) * 15);
        }

        public static TimePeriod tryAllPossibleTimePeriods(TimeSpan chargeDuration, IList<TimePeriod> possibleChargePeriods, double chargePower,
            IEnumerable<KeyValuePair<int, ApprovedBooking>> approvedBookings,
            string chargeGroupName, string chargeStationName, int chargePointIndex, Infrastructure infrastructure,
            ConnectorType connectorType, bool ignoreConnectorTypeReserve) {
            foreach (var timePeriod in possibleChargePeriods) {
                //timePeriods rounded to 15 min?
                for (DateTime startOfNewBooking = timePeriod.startTime;
                    startOfNewBooking + chargeDuration <= timePeriod.endTime;
                    startOfNewBooking += TimeSpan.FromMinutes(15)) {
                    var newBookingTimePeriod = new TimePeriod(startOfNewBooking, startOfNewBooking + chargeDuration);
                    if (doesAnotherBookingCollide(approvedBookings, infrastructure, chargeGroupName, chargeStationName, chargePointIndex,
                        newBookingTimePeriod)) {
                        continue;
                    }
                    if (!isChargeGroupMaxChargePowerNeverExceeded(newBookingTimePeriod, approvedBookings, infrastructure, chargePower,
                        chargeGroupName, connectorType)) {
                        continue;
                    }
                    if (!doesNotViolateConnectorTypeReserve(connectorType, approvedBookings, infrastructure, newBookingTimePeriod) &&
                        !ignoreConnectorTypeReserve) {
                        continue;
                    }
                    return newBookingTimePeriod;
                }
            }
            return null;
        }

        public static bool doesNotViolateConnectorTypeReserve(ConnectorType connectorType,
            IEnumerable<KeyValuePair<int, ApprovedBooking>> scheduledBookings,
            Infrastructure infrastructure, TimePeriod timePeriod) {
            int reservedConnectorsCount;
            if (connectorType == ConnectorType.CCS) {
                reservedConnectorsCount = infrastructure.infrastructureSettings.minReservedCCSConnectors;
            } else if (connectorType == ConnectorType.CHADEMO) {
                reservedConnectorsCount = infrastructure.infrastructureSettings.minReservedChademo;
            } else if (connectorType == ConnectorType.TYPE_TWO) {
                reservedConnectorsCount = infrastructure.infrastructureSettings.minReservedType2;
            } else {
                //all other connectors does not have reservation
                return true;
            }
            var allConnectorsCount = infrastructure.getAvailableChargePointsByConnectorType(connectorType).Count;

            for (DateTime dateTimeInBetween = timePeriod.startTime;
                dateTimeInBetween <= timePeriod.endTime;
                dateTimeInBetween += TimeSpan.FromMinutes(15)) {
                var takenConnectorCount = scheduledBookings
                    .Where(pair => pair.Value.timePeriod.startTime <= dateTimeInBetween &&
                                   pair.Value.timePeriod.endTime >= dateTimeInBetween + TimeSpan.FromMinutes(15))
                    .Count(x => x.Value.connectorType == connectorType);
                if (allConnectorsCount - takenConnectorCount <= reservedConnectorsCount) {
                    return false;
                }
            }
            return true;
        }

        public static bool isChargeGroupMaxChargePowerNeverExceeded(TimePeriod timePeriod,
            IEnumerable<KeyValuePair<int, ApprovedBooking>> approvedBookings,
            Infrastructure infrastructure, double plannedChargePower, string chargeGroupName, ConnectorType connectorType) {
            for (DateTime dateTimeInBetween = timePeriod.startTime;
                dateTimeInBetween <= timePeriod.endTime;
                dateTimeInBetween += TimeSpan.FromMinutes(15)) {
                var chargeGroupWorkLoad = approvedBookings.Where(pair => pair.Value.chargeGroupName == chargeGroupName)
                    .Where(pair => pair.Value.timePeriod.startTime <= dateTimeInBetween &&
                                   pair.Value.timePeriod.endTime >= dateTimeInBetween + TimeSpan.FromMinutes(15))
                    .Sum(pair => Math.Min(pair.Value.car.connectors[pair.Value.connectorType], infrastructure.chargeGroups[pair.Value.chargeGroupName]
                        .chargeStations[pair.Value.chargeStationName]
                        .chargePoints[pair.Value.chargePointIndex].connectors[pair.Value.connectorType]));
                if (chargeGroupWorkLoad + plannedChargePower > infrastructure.chargeGroups[chargeGroupName].maxChargePower) {
                    return false;
                }
            }
            return true;
        }

        public static void distributeAdHoc(int bookingId, RequestedBooking booking, Infrastructure infrastructure, IBookingManager bookingManager,
            string locationName) {
            lock (bookingManager) {
                var scheduledBookings = getScheduledBookings(locationName, bookingManager);
                tryToApproveBooking(bookingId, booking, infrastructure, bookingManager, scheduledBookings, true);
            }
        }

        public static IList<KeyValuePair<int, ApprovedBooking>> getScheduledBookings(string locationName, IBookingManager bookingManager) {
            var bookings = bookingManager.getBookingsByLocation(locationName);
            return bookings.Where(pair => pair.Value is ApprovedBooking)
                .Select(pair => new KeyValuePair<int, ApprovedBooking>(pair.Key, pair.Value as ApprovedBooking))
                .Where(pair => pair.Value.status == ApprovedBookingStatus.SCHEDULED ||
                               pair.Value.status == ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED).ToList();
        }
    }
}