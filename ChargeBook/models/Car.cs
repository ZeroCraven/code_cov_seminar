using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using chargebook.attributes;

namespace chargebook.models {
    public class Car {
        [StringLength(60, ErrorMessage = "Der Name des Fahrzeugs darf nicht länger als 60 sein")]
        [Required(ErrorMessage = "Der Name darf nicht leer sein")]
        public string name { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Die Batteriekapazität muss größer 0 sein")]
        public double batteryCapacity { get; set; }

        [EntryInDictionaryRequiredValidation(ErrorMessage = "Es muss mindestens ein Stecker für das Fahrzeug angegeben werden")]
        [ConnectorPowerDictionaryPowerRangeValidation(1, double.MaxValue, ErrorMessage = "Die maximale Leistung eines Steckers muss größer 0 sein")]

        public Dictionary<ConnectorType, double> connectors { get; set; }

        public Car() {
            connectors = new Dictionary<ConnectorType, double>();
        }
    }
}