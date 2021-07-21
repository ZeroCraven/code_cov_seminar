using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using ChargeBook.attributes.validationAttributes;
using chargebook.models;

namespace ChargeBook.models.booking {
    public abstract class Booking {

        [EmailAddress] public string email { get; set; }

        [LocationExistsValidation] public string location { get; set; }

        public Car car { get; set; }

        [Range(0, 1)] public double targetSoC { get; set; }
        [Range(0, 1)] public double startSoC { get; set; }

        public double getRequestedChargeEnergy() {
            return (targetSoC - startSoC) * car.batteryCapacity;
        }

        public abstract Booking deepCopy();
    }
}