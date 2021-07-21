using System.ComponentModel.DataAnnotations;

namespace ChargeBook.viewModels.simulationViewModels.infrastructureViewModels {
    public class SimulationCreateChargeGroupViewModel {
        [Required(ErrorMessage = "Kein Name für die Ladegruppe angegeben")]
        [RegularExpression("[a-zA-Z]{1,3}", ErrorMessage = "Für den Namen einer Ladegruppe sind maximal 3 Buchstaben zulässig")]
        public string chargeGroupName { get; set; }

        [Required(ErrorMessage = "Kein maximaler Ladewert angegeben")]
        [Range(0, int.MaxValue, ErrorMessage = "Der maximale Ladewert liegt zwischen 0 und 2.147.483.647")]
        public int maxChargePower { get; set; }
    }
}