using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace chargebook.attributes {
    public class EntryInDictionaryRequiredValidationAttribute : ValidationAttribute {
        public override bool IsValid(object value) {
            return value is IDictionary dictionary && dictionary.Count > 0;
        }
    }
}