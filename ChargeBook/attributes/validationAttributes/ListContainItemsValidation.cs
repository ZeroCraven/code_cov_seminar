using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ChargeBook.attributes.validationAttributes {
    public class ListContainItemsValidation : ValidationAttribute {
        public override bool IsValid(object value) {
            return value is IList list && list.Count > 0;
        }
    }
}