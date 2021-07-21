using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using chargebook.models;

namespace chargebook.attributes {
    public class ConnectorPowerDictionaryPowerRangeValidationAttribute : ValidationAttribute {
        private readonly double minValue;
        private readonly double maxValue;

        public ConnectorPowerDictionaryPowerRangeValidationAttribute(double minValue, double maxValue) {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override bool IsValid(object value) {
            return value is IDictionary<ConnectorType, double> dictionary && !dictionary.Values.Any(x => minValue > x || maxValue < x);
        }
    }
}