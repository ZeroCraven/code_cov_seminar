using System.ComponentModel.DataAnnotations;
using chargebook.data.user;
using Microsoft.Extensions.DependencyInjection;

namespace ChargeBook.attributes.validationAttributes {
    public class PriorityRoleExitsValidationAttribute : ValidationAttribute {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var userManager = validationContext.GetRequiredService<IUserManager>();
            if (value is string priorityRole && userManager.possiblePriorityRoles.ContainsKey(priorityRole)) {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}