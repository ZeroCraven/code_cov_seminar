using System;

namespace ChargeBook.models.booking.bookingExceptions {
    public class BookingNotFoundException : Exception {
        public BookingNotFoundException() { }

        public BookingNotFoundException(string message)
            : base(message) { }

        public BookingNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }
}