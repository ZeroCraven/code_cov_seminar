using System.ComponentModel.DataAnnotations;
using chargebook.data.user;
using Microsoft.Extensions.DependencyInjection;

namespace ChargeBook.attributes.validationAttributes {
    public class CarExistsValidationAttribute : ValidationAttribute {
        private readonly string userEmail;

        public CarExistsValidationAttribute(string userEmail) {
            this.userEmail = userEmail;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var userManager = validationContext.GetRequiredService<IUserManager>();
            if (validationContext.ObjectType.GetProperty(userEmail) == null) {
                return new ValidationResult(ErrorMessage);
            }
            string email =
                (string) validationContext.ObjectType.GetProperty(userEmail).GetValue(validationContext.ObjectInstance);
            if (value is string carName && userManager.existsCar(email, carName)) {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }

    }
}