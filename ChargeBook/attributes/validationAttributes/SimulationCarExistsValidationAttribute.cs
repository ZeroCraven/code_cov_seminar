using System.ComponentModel.DataAnnotations;
using chargebook.data.simulation;
using Microsoft.Extensions.DependencyInjection;

namespace ChargeBook.attributes.validationAttributes {
    public class SimulationCarExistsValidationAttribute : ValidationAttribute {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var simulationCarsManager = validationContext.GetRequiredService<SimulationCarsManager>();
            if (value is string carName && simulationCarsManager.existsCar(carName)) {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}