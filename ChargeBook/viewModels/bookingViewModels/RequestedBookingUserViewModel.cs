using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;
using ChargeBook.models.booking;

namespace ChargeBook.viewModels.bookingViewModels {
    public class RequestedBookingUserViewModel {

        [TimePeriodListInFutureValidation(ErrorMessage = "Angegebene Zeitspannen müssen in der Zukunft liegen")]
        [TimePeriodListPositiveTimespanValidation(ErrorMessage = "Der Startzeitpunkt jeder Zeitspanne muss vor dem Endzeitpunkt liegen")]
        public List<TimePeriod> timePeriods { get; set; }

        [Required(ErrorMessage = "Bitte geben Sie ein Auto an")]
        public string carName { get; set; }

        [LocationExistsValidation(ErrorMessage = "Der angegebene Standort ist nicht im System vorhanden")]
        public string location { get; set; }

        [CompareGreaterOrLowerValidation(nameof(targetSoC), false, ErrorMessage = "Der Start-SoC muss kleiner als der Ziel-SoC sein")]
        [Range(0, 100, ErrorMessage = "Der Startladewert muss einer Prozentzahl zwischen 0 und 100 entsprechen")]
        public int startSoC { get; set; }

        [CompareGreaterOrLowerValidation(nameof(startSoC), true, ErrorMessage = "Der Ziel-SoC muss größer als der Start-SoC sein")]
        [Range(0, 100, ErrorMessage = "Der Endladewert muss einer Prozentzahl zwischen 0 und 100 entsprechen")]
        public int targetSoC { get; set; }

        public RequestedBookingUserViewModel() {
            timePeriods = new List<TimePeriod>();
        }
    }

}