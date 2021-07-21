using System;
using System.ComponentModel.DataAnnotations;

namespace ChargeBook.viewModels.simulationViewModels {
    public class RequestedSimulationTimePeriodViewModel {
        [Required(ErrorMessage = "Der Starzeitpunkt muss angegeben werden")]
        [Range(0, 24 * 60, ErrorMessage = "Der Startzeitpunkt darf nur zwischen 0 und 24 Uhr liegen")]
        public int minutesFromMidnightStart { get; set; }

        [Required(ErrorMessage = "Der Endzeitpunkt muss angegeben werden")]
        [Range(0, 24 * 60, ErrorMessage = "Der Endzeitpunkt darf nur zwischen 0 und 24 Uhr liegen")]
        public int minutesFromMidnightEnd { get; set; }

    }
}