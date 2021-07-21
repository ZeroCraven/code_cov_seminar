using System;
using System.ComponentModel.DataAnnotations;

namespace ChargeBook.attributes.validationAttributes {
    public class CompareGreaterOrLowerValidationAttribute : ValidationAttribute {
        private readonly string comparedModelMember;
        private readonly bool shouldBeGreater;

        public CompareGreaterOrLowerValidationAttribute(string comparedModelMember, bool shouldBeGreater) {
            this.comparedModelMember = comparedModelMember;
            this.shouldBeGreater = shouldBeGreater;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            if (validationContext.ObjectType.GetProperty(comparedModelMember) == null) return new ValidationResult(ErrorMessage);
            var compareToValue =
                (int) validationContext.ObjectType.GetProperty(comparedModelMember).GetValue(validationContext.ObjectInstance);
            if (shouldBeGreater) {
                return compareToValue < ((int) value) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
            } else {
                return compareToValue > ((int) value) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
            }
        }
    }
}