using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace chargebook.viewModels.infrastructure {
    public class CreateLocationViewModel {
        [Required(ErrorMessage = "Kein Standort angegeben")]
        public string location { get; set; }

        [Required(ErrorMessage = "Keine Zeitzone angegeben")]
        [TimeZoneInfoFormatValidation(ErrorMessage = "Die Zeitzone konnte im System nicht gefunden werden")]
        public string timeZoneId { get; set; }
    }
}