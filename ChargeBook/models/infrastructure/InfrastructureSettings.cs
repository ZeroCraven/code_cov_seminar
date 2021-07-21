using System;
using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace chargebook.models.infrastructure {
    public class InfrastructureSettings {
        public int minReservedCCSConnectors { get; set; }
        public int minReservedType2 { get; set; }
        public int minReservedChademo { get; set; }
        public int beginBuffer { get; set; }
        public int endBuffer { get; set; }
        public TimeSpan bookingDistributorActivationTime { get; set; }
    }
}