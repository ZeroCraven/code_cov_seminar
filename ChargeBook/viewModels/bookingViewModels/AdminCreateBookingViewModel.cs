using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ChargeBook.attributes.validationAttributes;
using ChargeBook.attributes.validationAttributes.AdminCreateBookingViewModel;
using ChargeBook.models.booking;

namespace ChargeBook.viewModels.bookingViewModels {
    public class AdminCreateBookingViewModel {
        public int? id { get; set; }

        [StatusValidation(ErrorMessage = "Der übergebene Status konnte im System nicht gefunden werden")]
        public string status { get; set; }

        [TimePeriodListInFutureAdminValidation(nameof(status), ErrorMessage = "Angegebene Zeitspannen müssen in der Zukunft liegen")]
        [TimePeriodListPositiveTimespanValidation(ErrorMessage = "Der Startzeitpunkt jeder Zeitspanne muss vor dem Endzeitpunkt liegen")]
        public List<TimePeriod> timePeriods { get; set; }

        [EmailExistsValidation(ErrorMessage = "Diese E-Mailadresse existiert im System nicht")]
        public string user { get; set; }

        [CarExistsValidation(nameof(user), ErrorMessage = "Das angegebene Auto wurde im System nicht gefunden")]
        public string car { get; set; }

        [LocationExistsValidation(ErrorMessage = "Der angegebene Standort ist nicht im System vorhanden")]
        public string location { get; set; }

        [CompareGreaterOrLowerValidation(nameof(targetSoC), false, ErrorMessage = "Der Start-SoC muss kleiner als der Ziel-SoC sein")]
        [Range(0, 100, ErrorMessage = "Der Startladewert muss einer Prozentzahl zwischen 0 und 100 entsprechen")]
        public int startSoC { get; set; }

        [Range(0, 100, ErrorMessage = "Der Endladewert muss einer Prozentzahl zwischen 0 und 100 entsprechen")]
        public int targetSoC { get; set; }
        
        public string? chargeGroupName { get; set; }
        public string? chargeStationName { get; set; }
        public int? chargePointIndex  { get; set; }
        public string? connectorType { get; set; }
    }
    
}