using System.ComponentModel.DataAnnotations;
using chargebook.data.user;
using Microsoft.Extensions.DependencyInjection;

namespace ChargeBook.attributes.validationAttributes {
    public class EmailExistsValidationAttribute : ValidationAttribute {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var userManager = validationContext.GetRequiredService<IUserManager>();
            if (userManager == null) {
                return new ValidationResult(ErrorMessage);
            }
            if (value is string email && userManager.existsUser(email)) {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}