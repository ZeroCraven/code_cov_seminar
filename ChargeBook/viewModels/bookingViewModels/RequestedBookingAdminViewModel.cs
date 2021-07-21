using ChargeBook.attributes.validationAttributes;

namespace ChargeBook.viewModels.bookingViewModels {
    public class RequestedBookingAdminViewModel : RequestedBookingUserViewModel {

        [EmailExistsValidation(ErrorMessage = "Diese E-Mailadresse existiert nicht im System")]
        public string email { get; set; }

        public bool denied { get; set; }
        public RequestedBookingAdminViewModel() : base() { }
    }
}