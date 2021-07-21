using System.ComponentModel.DataAnnotations;

namespace ChargeBook.viewModels.simulationViewModels.infrastructureViewModels {
    public class SimulationDeleteChargeGroupViewModel {
        [Required(ErrorMessage = "Kein Name für die Ladegruppe angegeben")]
        [RegularExpression("[a-zA-Z]{1,3}", ErrorMessage = "Für den Namen einer Ladegruppe sind maximal 3 Buchstaben zulässig")]
        public string chargeGroupName { get; set; }
    }
}