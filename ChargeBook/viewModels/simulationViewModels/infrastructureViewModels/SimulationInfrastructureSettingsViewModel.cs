using System;
using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace ChargeBook.viewModels.simulationViewModels.infrastructureViewModels {
    public class SimulationInfrastructureSettingsViewModel {

        [Required(ErrorMessage = "Kein Wert für die reservierten CCS-Stecker angegeben")]
        [Range(0, Int32.MaxValue, ErrorMessage = "Der maximale Wert für die reservierten CCS-Stecker liegt zwischen 0 und 2.147.483.647")]
        public int minReservedCCSConnectors { get; set; }

        [Required(ErrorMessage = "Kein Wert für die reservierten Type2-Stecker angegeben")]
        [Range(0, Int32.MaxValue, ErrorMessage = "Der maximale Wert für die reservierten Type2-Stecker liegt zwischen 0 und 2.147.483.647")]
        public int minReservedType2 { get; set; }

        [Required(ErrorMessage = "Kein Wert für die reservierten CHAdeMO-Stecker angegeben")]
        [Range(0, Int32.MaxValue, ErrorMessage = "Der maximale Wert für die reservierten CHAdeMO-Stecker liegt zwischen 0 und 2.147.483.647")]
        public int minReservedChademo { get; set; }

        [Required(ErrorMessage = "Keine Zeit für den Startpuffer angegeben")]
        [Range(0, 60, ErrorMessage = "Der Startpuffer muss zwischen 0 und 60 Minuten liegen")]
        [MultiplesOfValidation(5, ErrorMessage = "Der Startpuffer muss einem Vielfachen von 5 entsprechen")]
        public int beginBuffer { get; set; }

        [Required(ErrorMessage = "Keine Zeit für den Endpuffer angegeben")]
        [Range(0, 60, ErrorMessage = "Der Endpuffer muss zwischen 0 und 60 Minuten liegen")]
        [MultiplesOfValidation(15, ErrorMessage = "Der Endpuffer muss einem Vielfachen von 15 entsprechen")]
        public int endBuffer { get; set; }
    }
}