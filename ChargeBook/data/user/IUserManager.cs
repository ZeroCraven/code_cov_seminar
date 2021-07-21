using System;
using System.Collections.Generic;
using System.Security.Claims;
using chargebook.models;
using chargebook.viewModels;
using ChargeBook.viewModels.userViewModels;

namespace chargebook.data.user {

    public interface IUserManager {
        IReadOnlyDictionary<string, int> possiblePriorityRoles { get; }
        ClaimsIdentity findIdentity(string email, string password);
        ClaimsIdentity getIdentity(string email);
        void changeSelectedOnBehalf(string email, string onBehalf);
        void ensureUserExists(User user, string password);
        bool isUpToDate(string email, long lastChanged);
        IList<string> getAllUserEmails();
        string getUsername(string email);

        /// <summary>
        /// Tries to change the password of the given user. Returns true if it succeeded, else returns false if old password did not match.
        /// </summary>
        bool changePassword(string email, string oldPassword, string newPassword);

        void addCar(string email, Car car);
        void deleteCar(string email, string carName);
        bool isNotificationEnabled(string email);
        public string createUnverifiedUser(string email, bool isAdmin, string priorityRole);

        void registerUnverifiedUser(string email, string verificationToken, string password, string firstName, string lastName,
            string defaultLocation);

        bool checkUnverifiedUserVerificationToken(string email, string token);
        string prepareForPasswordReset(string email);
        void resetPassword(string email, string resetPasswordToken, string newPassword);
        bool checkResetPasswordToken(string email, string resetPasswordToken);
        public bool existsCar(string email, string carName);
        public bool existsUser(string email);
        public string getDefaultLocation(string email);
        public List<Car> getCars(string email);
        public IList<VerifiedUserViewModel> getVerifiedUsersAsViewModel();
        public IList<UnverifiedUserViewModel> getUnverifiedUsersAsViewModel();
        public void setDefaultLocation(string email, string location);
        public void setNotificationEnabled(string email, bool enabled);
        public int getPriority(string email);
        public IReadOnlyDictionary<string, int> getPriorityRoles();
        void editUser(string email, string firstName, string lastName, string priorityRole, bool isAdmin);
        void deleteUnverifiedUser(string email);
        void deleteVerifiedUser(string email);
        void handleLocationDeletion(string location);
    }
}