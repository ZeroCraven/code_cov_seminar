using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using chargebook.models;
using chargebook.viewModels;
using ChargeBook.viewModels.userViewModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Org.BouncyCastle.Security;

namespace chargebook.data.user {
    public class UserManager : IUserManager {
        protected Dictionary<string, User> users;
        protected Dictionary<string, UnverifiedUser> unverifiedUsers;
        protected Dictionary<string, UserUrlToken> passwordForgottenUsers;
        public IReadOnlyDictionary<string, int> possiblePriorityRoles { get; }

        public UserManager(Dictionary<string, int> possiblePriorityRoles) {
            users = new Dictionary<string, User>();
            unverifiedUsers = new Dictionary<string, UnverifiedUser>();
            passwordForgottenUsers = new Dictionary<string, UserUrlToken>();
            this.possiblePriorityRoles = possiblePriorityRoles;
        }


        public ClaimsIdentity findIdentity(string email, string password) {
            lock (this) {
                if (users.ContainsKey(email)) {
                    var user = users[email];
                    var hashedInputPassword = hashPassword(password, user.salt);
                    if (user.password == hashedInputPassword) {
                        return new ClaimsIdentity(user.getClaims(), "default");
                    }
                }
                return null;
            }
        }

        public ClaimsIdentity getIdentity(string email) {
            lock (this) {
                if (users.ContainsKey(email)) {
                    var user = users[email];
                    return new ClaimsIdentity(user.getClaims(), "default");
                }
                return null;
            }
        }

        public virtual void changeSelectedOnBehalf(string email, string onBehalf) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                var user = users[email];
                if (!user.onBehalfs.Contains(onBehalf)) {
                    throw new UserNotAuthorisedException();
                }
                user.lastChanged = DateTime.UtcNow.Ticks;
                user.selectedOnBehalf = onBehalf;
            }
        }

        public void ensureUserExists(User user, string password) {
            lock (this) {
                user.salt = generateSalt();
                user.password = hashPassword(password, user.salt);
                user.lastChanged = DateTime.UtcNow.Ticks;
                users[user.email] = user;
            }
        }

        public bool isUpToDate(string email, long lastChanged) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                return users[email].lastChanged <= lastChanged;
            }
        }

        public IList<string> getAllUserEmails() {
            lock (this) {
                return new List<string>(users.Keys).AsReadOnly();
            }
        }

        public string getUsername(string email) {
            lock (this) {
                return users[email].lastName;
            }
        }

        /// <summary>
        /// Tries to change the password of the given user. Returns true if it succeeded, else returns false if old password did not match.
        /// </summary>
        public virtual bool changePassword(string email, string oldPassword, string newPassword) {
            lock (this) {
                var user = users[email];
                var oldPasswordHash = hashPassword(oldPassword, user.salt);
                if (user.password != oldPasswordHash) {
                    return false;
                }
                var newPasswordHash = hashPassword(newPassword, user.salt);
                users[email].password = newPasswordHash;
                return true;
            }
        }

        public virtual void addCar(string email, Car car) {
            lock (this) {
                if (users[email].ownedCars.Exists(x => x.name == car.name)) {
                    throw new ArgumentException("Car already Exists");
                }
                users[email].ownedCars.Add(car);
            }
        }

        public virtual void deleteCar(string email, string carName) {
            lock (this) {
                users[email].ownedCars.RemoveAll(car => car.name == carName);
            }
        }

        public bool existsUser(string email) {
            lock (this) {
                try {
                    return users.ContainsKey(email);
                }
                catch (ArgumentNullException) {
                    return false;
                }
            }
        }

        public bool existsCar(string email, string carName) {
            lock (this) {
                return existsUser(email) && users[email].ownedCars.Any(x => x.name == carName);
            }
        }

        public bool isNotificationEnabled(string email) {
            lock (this) {
                return users[email].notificationEnabled;
            }
        }

        private string hashPassword(string password, byte[] salt) {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256
            ));
        }

        private byte[] generateSalt() {
            var salt = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }


        public virtual string createUnverifiedUser(string email, bool isAdmin, string priorityRole) {
            lock (this) {
                if (users.ContainsKey(email)) {
                    throw new UserAlreadyExistsException();
                }
                unverifiedUsers[email] = new UnverifiedUser() {
                    token = new UserUrlToken(), isAdmin = isAdmin, priorityRole = priorityRole
                };
                return unverifiedUsers[email].token.validationToken;
            }
        }

        public virtual void registerUnverifiedUser(string email, string verificationToken, string password, string firstName, string lastName,
            string defaultLocation) {
            lock (this) {
                if (!unverifiedUsers.ContainsKey(email) || !unverifiedUsers[email].token.doesMatchAndIsNotExpired(verificationToken)) {
                    throw new ValidTokenEmailCombinationNotFoundException();
                }
                var salt = generateSalt();
                users.Add(email, new User {
                    email = email,
                    salt = salt,
                    password = hashPassword(password, salt),
                    isAdmin = unverifiedUsers[email].isAdmin,
                    lastChanged = DateTime.UtcNow.Ticks,
                    firstName = firstName,
                    lastName = lastName,
                    defaultLocation = defaultLocation,
                    notificationEnabled = true,
                    onBehalfs = new List<string>() {
                        email
                    },
                    priorityRole = unverifiedUsers[email].priorityRole,
                    selectedOnBehalf = email,
                });
                unverifiedUsers.Remove(email);
            }
        }

        public bool checkUnverifiedUserVerificationToken(string email, string token) {
            lock (this) {
                return unverifiedUsers.ContainsKey(email) && unverifiedUsers[email].token.doesMatchAndIsNotExpired(token);
            }
        }

        public virtual string prepareForPasswordReset(string email) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                passwordForgottenUsers[email] = new UserUrlToken();
                return passwordForgottenUsers[email].validationToken;
            }
        }

        public virtual void resetPassword(string email, string resetPasswordToken, string newPassword) {
            lock (this) {
                if (!passwordForgottenUsers.ContainsKey(email)) {
                    throw new ValidTokenEmailCombinationNotFoundException();
                }
                if (!passwordForgottenUsers[email].doesMatchAndIsNotExpired(resetPasswordToken)) {
                    throw new ValidTokenEmailCombinationNotFoundException();
                }
                passwordForgottenUsers.Remove(email);
                var user = users[email];
                user.password = hashPassword(newPassword, user.salt);
            }
        }

        public string getDefaultLocation(string email) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                return users[email].defaultLocation;
            }
        }

        public bool checkResetPasswordToken(string email, string resetPasswordToken) {
            lock (this) {
                return passwordForgottenUsers.ContainsKey(email) && passwordForgottenUsers[email].doesMatchAndIsNotExpired(resetPasswordToken);
            }
        }

        public List<Car> getCars(string email) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                return new List<Car>(users[email].ownedCars);
            }
        }

        public IList<VerifiedUserViewModel> getVerifiedUsersAsViewModel() {
            lock (this) {
                return users.Select(x => VerifiedUserViewModel.fromUser(x.Key, x.Value)).ToList();
            }
        }

        public IList<UnverifiedUserViewModel> getUnverifiedUsersAsViewModel() {
            lock (this) {
                return unverifiedUsers.Select(x => UnverifiedUserViewModel.fromUnverifiedUser(x.Key, x.Value)).ToList();
            }
        }

        public virtual void setDefaultLocation(string email, string location) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                users[email].defaultLocation = location;
            }
        }

        public virtual void setNotificationEnabled(string email, bool enabled) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                users[email].notificationEnabled = enabled;
            }
        }

        public IReadOnlyDictionary<string, int> getPriorityRoles() {
            return possiblePriorityRoles;
        }

        public int getPriority(string email) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    return 0;
                }
                if (!possiblePriorityRoles.ContainsKey(users[email].priorityRole)) {
                    return 0;
                }
                return possiblePriorityRoles[users[email].priorityRole];
            }
        }

        public virtual void editUser(string email, string firstName, string lastName, string priorityRole, bool isAdmin) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                var user = users[email];
                user.firstName = firstName;
                user.lastName = lastName;
                user.priorityRole = priorityRole;
                user.isAdmin = isAdmin;
                user.lastChanged = DateTime.UtcNow.Ticks;
            }
        }

        public virtual void deleteUnverifiedUser(string email) {
            lock (this) {
                if (!unverifiedUsers.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                unverifiedUsers.Remove(email);
            }
        }

        public virtual void deleteVerifiedUser(string email) {
            lock (this) {
                if (!users.ContainsKey(email)) {
                    throw new UserNotFoundException();
                }
                users.Remove(email);
            }
        }

        public void handleLocationDeletion(string location) {
            lock (this) {
                users.Where(x => x.Value.defaultLocation == location)
                    .ToList().ForEach(x => x.Value.defaultLocation = "");
            }
        }
    }


    #region UserExceptions

    public class UserAlreadyExistsException : Exception {
        public UserAlreadyExistsException() : base() { }
    }

    public class ValidTokenEmailCombinationNotFoundException : Exception {
        public ValidTokenEmailCombinationNotFoundException() : base() { }
    }

    #endregion

}