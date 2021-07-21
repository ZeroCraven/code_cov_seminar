using System;
using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace chargebook.viewModels.infrastructure {
    public class CreateChargeGroupViewModel {
        [Required(ErrorMessage = "Kein Name für die Ladegruppe angegeben")]
        [RegularExpression("[a-zA-Z]{1,3}", ErrorMessage = "Für den Namen einer Ladegruppe sind maximal 3 Buchstaben erlaubt")]
        public string chargeGroupName { get; set; }

        [Required(ErrorMessage = "Kein Standort angegeben")]
        [LocationExistsValidation(ErrorMessage = "Der Standort konnte im System nicht gefunden werden")]
        public string location { get; set; }

        [Required(ErrorMessage = "Kein maximaler Ladewert angegeben")]
        [Range(1, int.MaxValue, ErrorMessage = "Der maximale Ladewert liegt zwischen 1 und 2.147.483.647")]
        public int maxChargePower { get; set; }
    }
}