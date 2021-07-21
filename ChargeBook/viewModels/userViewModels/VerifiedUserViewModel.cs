using chargebook.data.user;

namespace ChargeBook.viewModels.userViewModels {
    public class VerifiedUserViewModel {
        public string email;
        public bool isAdmin;
        public string firstName;
        public string lastName;
        public string priorityRole;
        public string defaultLocation;

        public static VerifiedUserViewModel fromUser(string email, User user) {
            return new VerifiedUserViewModel() {
                email = email,
                isAdmin = user.isAdmin,
                firstName = user.firstName,
                lastName = user.lastName,
                priorityRole = user.priorityRole,
                defaultLocation = user.defaultLocation
            };
        }
    }

}