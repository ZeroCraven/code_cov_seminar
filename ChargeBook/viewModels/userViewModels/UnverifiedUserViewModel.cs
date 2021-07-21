using System;
using chargebook.data.user;

namespace ChargeBook.viewModels.userViewModels {
    public class UnverifiedUserViewModel {
        public string email;
        public string priorityRole;
        public bool isAdmin;
        public int daysUntilTokenExpires;

        public static UnverifiedUserViewModel fromUnverifiedUser(string email, UnverifiedUser unverifiedUser) {
            var daysUntilTokenExpires = (unverifiedUser.token.dateOfExpiry - DateTime.UtcNow).Days;
            if (unverifiedUser.token.dateOfExpiry < DateTime.UtcNow)
                daysUntilTokenExpires = Math.Min(daysUntilTokenExpires, -1);
            return new UnverifiedUserViewModel() {
                email = email,
                isAdmin = unverifiedUser.isAdmin,
                priorityRole = unverifiedUser.priorityRole,
                daysUntilTokenExpires = daysUntilTokenExpires,
            };
        }
    }
}