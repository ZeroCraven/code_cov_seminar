using System.ComponentModel.DataAnnotations;

namespace ChargeBook.attributes.validationAttributes {
    public class MultiplesOfValidationAttribute : ValidationAttribute {
        private readonly int factor;

        public MultiplesOfValidationAttribute(int factor) {
            this.factor = factor;
        }

        public override bool IsValid(object value) {
            return value is int val && val % factor == 0;
        }

    }
}