using System;

namespace ChargeBook.models.booking.bookingExceptions {
    public class WrongBookingTypeException : Exception {
        public WrongBookingTypeException() { }

        public WrongBookingTypeException(string message)
            : base(message) { }

        public WrongBookingTypeException(string message, Exception inner)
            : base(message, inner) { }
    }
}