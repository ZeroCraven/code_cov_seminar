using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace ChargeBook.viewModels.simulationViewModels.infrastructureViewModels {
    public class SimulationCreateChargeStationViewModel {
        [Required(ErrorMessage = "Kein Name für die Ladegruppe angegeben")]
        [RegularExpression("[a-zA-Z]{1,3}", ErrorMessage = "Für den Namen einer Ladegruppe sind maximal 3 Buchstaben zulässig")]
        public string chargeGroupName { get; set; }

        [Required(ErrorMessage = "Kein Name für die Ladestation angegeben")]
        [RegularExpression("[0-9]{1,3}", ErrorMessage = "Für den Namen einer Ladestation sind maximal 3 Zahlen zulässig")]
        public string chargeStationName { get; set; }

        [Required(ErrorMessage = "Kein Name für den Typ der Ladestation angegeben")]
        [ChargeStationTypeStringFormatValidation(ErrorMessage = "Der Typ der Ladestation ist nicht gültig")]
        public string chargeStationTypeName { get; set; }
    }
}