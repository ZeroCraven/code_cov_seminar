using System;
using ChargeBook.models.simulation.settings;
using ChargeBook.models.simulation.simulationLog;

namespace ChargeBook.viewModels.simulationViewModels {
    public class SimulationPreviewViewModel {
        public int id { get; set; }
        public string creatorEmail { get; set; }
        public GeneralSettings generalSettings { get; set; }
        public Statistics statistics { get; set; }

        public DateTime startTime { get; set; }
    }
}