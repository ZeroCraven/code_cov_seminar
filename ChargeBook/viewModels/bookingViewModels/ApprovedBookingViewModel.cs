using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;
using chargebook.models;
using ChargeBook.models.booking;

namespace ChargeBook.viewModels.bookingViewModels {
    public class ApprovedBookingViewModel {


        [EmailExistsValidation(ErrorMessage = "Diese E-Mailadresse existiert nicht im System")]
        public string email { get; set; }

        [Required(ErrorMessage = "Kein Auto angegeben")]
        [CarExistsValidation(nameof(email), ErrorMessage = "Das angegebene Auto wurde im System nicht gefunden")]
        public string carName { get; set; }

        [LocationExistsValidation(ErrorMessage = "Der angegebene Standort ist nicht im System vorhanden")]
        public string location { get; set; }

        [CompareGreaterOrLowerValidation(nameof(targetSoC), false, ErrorMessage = "Der Start-SoC muss kleiner als der Ziel-SoC sein")]
        [Range(0, 100, ErrorMessage = "Der Startladewert muss einer Prozentzahl zwischen 0 und 100 entsprechen")]
        public int startSoC { get; set; }

        [CompareGreaterOrLowerValidation(nameof(startSoC), true, ErrorMessage = "Der Ziel-SoC muss größer als der Start-SoC sein")]
        [Range(0, 100, ErrorMessage = "Der Endladewert muss einer Prozentzahl zwischen 0 und 100 entsprechen")]
        public int targetSoC { get; set; }

        public ApprovedBookingStatus status { get; set; }

        public string chargeGroupName { get; set; }

        public string chargeStationName { get; set; }

        public int chargePointIndex { get; set; }

        public ConnectorType connectorType { get; set; }

        [TimePeriodInFutureValidation(ErrorMessage = "Die angefragte Zeitspanne muss in der Zukunft liegen")]
        [TimePeriodPositiveTimeSpanValidation(ErrorMessage = "Der Startzeitpunkt der Zeitspanne muss vor dem Endzeitpunkt liegen")]
        public TimePeriod timePeriod { get; set; }
    }
}