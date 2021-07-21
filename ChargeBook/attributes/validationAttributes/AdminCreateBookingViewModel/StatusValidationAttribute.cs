using System.ComponentModel.DataAnnotations;

namespace ChargeBook.attributes.validationAttributes.AdminCreateBookingViewModel {
    public class StatusValidationAttribute : ValidationAttribute {

        public override bool IsValid(object value) {
            return (value is string status && (status == "angefordert" || status == "abgelehnt" || status == "abgelaufen" || status == "angenommen"
                                               || status == "Ladevorgang begonnen" || status == "Ladevorgang beendet"));
        }
    }
}