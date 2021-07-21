using chargebook.models.infrastructure;
using ChargeBook.models.simulation.settings;

namespace ChargeBook.models.simulation.simulationLog {
    public class Scenario {
        public Infrastructure simulatedInfrastructure { get; set; }
        public SimulationSettings simulationSettings { get; set; }
        
    }
}