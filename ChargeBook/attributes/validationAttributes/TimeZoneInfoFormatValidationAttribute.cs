using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ChargeBook.attributes.validationAttributes {
    public class TimeZoneInfoFormatValidationAttribute : ValidationAttribute {
        public override bool IsValid(object value) {
            if (value is string timeZoneInfoString && timeZoneInfoString != "") {
                try {
                    TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoString);
                }
                catch (TimeZoneNotFoundException) {
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}