using System.ComponentModel.DataAnnotations;
using chargebook.attributes;

namespace ChargeBook.models.booking {
    public enum ApprovedBookingStatus {
        [Display(Name = "angenommen")] [Color("#232423", "#232323")]
        SCHEDULED,

        [Display(Name = "Ladevorgang begonnen")] [Color("#232423", "#232323")]
        CHARGE_BEGIN_CONFIRMED,

        [Display(Name = "Ladevorgang beendet")] [Color("#232423", "#232323")]
        CHARGE_END_CONFIRMED,

        [Display(Name = "abgelaufen")] [Color("#232423", "#232332")]
        EXPIRED
    }
}