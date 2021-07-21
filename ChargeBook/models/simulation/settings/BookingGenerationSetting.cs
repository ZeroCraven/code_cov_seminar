using System;
using System.Collections.Generic;
using System.Linq;
using chargebook.models;
using ChargeBook.models.booking;

namespace ChargeBook.models.simulation.settings {
    public class BookingGenerationSetting {

        public Car car { get; set; }

        public int count { get; set; }

        public double requestFrequency { get; set; }

        public List<DayOfWeek> bookingDays { get; set; }

        public double chargedEnergyInPercent { get; set; }

        public TimeSpan timeOfRequest { get; set; }

        public List<Tuple<TimeSpan, TimeSpan>> requestedTimeSpans { get; set; }

        public string priorityRole { get; set; }

        public bool isNotFullySet() {
            return (car == null || count < 0 || requestFrequency < 0 || bookingDays == null || chargedEnergyInPercent < 0 ||
                    timeOfRequest < TimeSpan.Zero
                    || requestedTimeSpans == null || requestedTimeSpans.Count == 0 || priorityRole == null || priorityRole == "");
        }

        public BookingGenerationSetting deepCopy() {
            return new BookingGenerationSetting() {
                bookingDays = bookingDays.Select(x => x).ToList(),
                car = car,
                chargedEnergyInPercent = chargedEnergyInPercent,
                count = count,
                priorityRole = priorityRole,
                requestedTimeSpans = requestedTimeSpans.Select(x => new Tuple<TimeSpan,TimeSpan>(x.Item1,x.Item2)).ToList(),
                requestFrequency = requestFrequency,
                timeOfRequest = timeOfRequest,
            };
        }
    }
}