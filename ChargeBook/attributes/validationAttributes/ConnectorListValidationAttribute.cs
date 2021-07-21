using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using chargebook.models;

namespace ChargeBook.attributes.validationAttributes {
    public class ConnectorListValidationAttribute : ValidationAttribute {

        public override bool IsValid(object value) {
            return value is Dictionary<ConnectorType, double> list && list.All(x => x.Value > 0);
        }
    }
}