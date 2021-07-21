using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace chargebook.models.infrastructure {
    public class ChargeGroup {
        public IDictionary<string, ChargeStationType> chargeStations { get; set; }
        [Required] public double maxChargePower { get; set; }

        public ChargeGroup() {
            chargeStations = new Dictionary<string, ChargeStationType>();
        }

    }
}