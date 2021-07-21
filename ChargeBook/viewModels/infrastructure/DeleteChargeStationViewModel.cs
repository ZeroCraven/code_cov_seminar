﻿using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace chargebook.viewModels.infrastructure {
    public class DeleteChargeStationViewModel {
        [Required(ErrorMessage = "Kein Name für die Ladegruppe angegeben")]
        [RegularExpression("[a-zA-Z]{1,3}", ErrorMessage = "Für den Namen einer Ladegruppe sind maximal 3 Buchstaben zulässig")]
        public string chargeGroupName { get; set; }

        [Required(ErrorMessage = "Kein Standort angegeben")]
        [LocationExistsValidation(ErrorMessage = "Der Standort konnte im System nicht gefunden werden")]
        public string location { get; set; }

        [Required(ErrorMessage = "Keine Zahlenfolge für die Ladestation angegeben")]
        [RegularExpression("[0-9]{1,3}", ErrorMessage = "Für den Namen einer Ladestation sind maximal 3 Zahlen erlaubt")]
        public string chargeStationName { get; set; }
    }
}