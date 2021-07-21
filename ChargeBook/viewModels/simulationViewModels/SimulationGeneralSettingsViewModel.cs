using System;
using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;
using ChargeBook.attributes.validationAttributes.simulation;
using ChargeBook.models.booking;

namespace chargebook.viewModels.simulationViewModels {
    public class SimulationGeneralSettingsViewModel {
        [Required(ErrorMessage = "Die Ticklänge muss angegeben werden")]
        [MultiplesOfValidation(15, ErrorMessage = "Die Ticklänge muss einem Vielfachen von 15 Minuten entsprechen")]
        [Range(15, int.MaxValue, ErrorMessage = "Die Ticklänge muss mindestens 15 Minuten sein")]
        public int tickLength { get; set; }

        [Required(ErrorMessage = "Die Zeitdauer für eine Simulation muss angegeben werden")]
        [TimePeriodMaxValueValidation(ErrorMessage = "Die Simulation darf nur über eine maximale Zeitspanne von 30 Tagen gehen")]
        [TimePeriodMinutesMultiplesOfValidation(60, ErrorMessage = "Start und Endzeit  dürfen nur in 60-Minuten-Schritten angegeben werden")]
        [TimePeriodPositiveTimeSpanSimulationValidation(ErrorMessage = "Die für die Simulation angegebene Zeitspanne muss positiv sein")]
        public TimePeriod timePeriod { get; set; }

        [Required(ErrorMessage = "Der Name einer Simulation muss angegeben werden")]
        [StringLength(100, ErrorMessage = "Der Name der Simulation darf maximal 100 Zeichen lang sein")]
        public string name { get; set; }

        [Required(ErrorMessage = "Der Seed einer Simulation muss angegeben werden")]
        [Range(0, int.MaxValue, ErrorMessage = "Der maximale Wert für den Seed liegt zwischen 0 und 2.147.483.647")]
        public int? seed { get; set; }
    }
}