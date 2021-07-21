using System;

namespace ChargeBook.models.booking.bookingExceptions {
    public class CollidingBookingException : Exception {
        public CollidingBookingException() { }

        public CollidingBookingException(string message)
            : base(message) { }

        public CollidingBookingException(string message, Exception inner)
            : base(message, inner) { }
    }
}