using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace chargebook.viewModels.infrastructure {
    public class LocationViewModel {
        [Required(ErrorMessage = "Kein Standort angegeben")]
        [LocationExistsValidation(ErrorMessage = "Der Standort konnte im System nicht gefunden werden")]
        public string location { get; set; }
    }
}