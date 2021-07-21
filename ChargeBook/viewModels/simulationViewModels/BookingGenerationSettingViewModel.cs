using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;
using ChargeBook.models.simulation;
using ChargeBook.viewModels.simulationViewModels;

namespace chargebook.viewModels.simulationViewModels {
    public class BookingGenerationSettingViewModel {

        [Required(ErrorMessage = "Kein Auto angegeben")]
        [SimulationCarExistsValidation(ErrorMessage = "Das angegebene Auto konnte im System nicht gefunden werden")]
        public string carName { get; set; }

        [Required(ErrorMessage = "Kein Standort angegeben")]
        [Range(1, int.MaxValue, ErrorMessage = "Die maximale Anzahl der Fahrzeuge liegt zwischen 1 und 2.147.483.647")]
        public int count { get; set; }

        [Required(ErrorMessage = "Kein Standort angegeben")]
        [Range(0, Double.MaxValue, ErrorMessage = "Der maximale Wert für die Nachfrage pro Woche liegt zwischen 0 und 2.147.483.647")]
        public double perWeekRequestFrequency { get; set; }

        [Required(ErrorMessage = "Kein Wochentag angegeben")]
        [ListContainItemsValidation(ErrorMessage = "Es sollte wenigstens ein möglicher Wochentag angegeben werden")]
        public List<DayOfWeek> bookingDays { get; set; }

        [Required(ErrorMessage = "Kein möglicher Ladewert angegeben")]
        [Range(0, 100, ErrorMessage = "Der maximale Wert für den möglichen Ladewert liegt zwischen 0 und 100")]
        public int chargedEnergyInPercent { get; set; }

        //number of hours the booking should start 
        [Required(ErrorMessage = "Kein Zeitraum für die Buchungsanfragen")]
        [Range(0, Int32.MaxValue,
            ErrorMessage = "Der maximale Wert für den Zeitraum zwischen Buchungsanfrage und Buchung  liegt zwischen 0 und 2.147.483.647")]
        public int requestTime { get; set; }

        [Required(ErrorMessage = "Kein Wert für Zeiträume angegeben")]
        [ListContainItemsValidation(ErrorMessage = "Es sollte wenigstens ein möglicher Wochentag angegeben werden")]
        public List<RequestedSimulationTimePeriodViewModel> requestedSimulationTimePeriods { get; set; }

        [Required(ErrorMessage = "Keine Prioritätsrolle angegeben")]
        [PriorityRoleExitsValidation(ErrorMessage = "Die angegeben Prioritätsrolle existiert nicht im System")]
        public string priorityRole { get; set; }

    }
}