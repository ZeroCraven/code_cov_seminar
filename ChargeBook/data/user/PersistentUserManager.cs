using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using chargebook.models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace chargebook.data.user {
    public class PersistentUserManager : UserManager, IDisposable {
        private readonly ManagerPersistor persistor;

        public PersistentUserManager(string pathToUsersFile, string pathToPossiblePriorityRoles, IConfiguration config) : base(
            readPossiblePriorityRolesFile(pathToPossiblePriorityRoles)) {
            var persistenceModel = readUserFile(pathToUsersFile);
            users = persistenceModel.users;
            unverifiedUsers = persistenceModel.unverifiedUser;
            passwordForgottenUsers = persistenceModel.passwordForgottenUser;


            ensureUserExists(new User() {
                email = "admin",
                firstName = "System",
                lastName = "Administrator",
                defaultLocation = "",
                isAdmin = true,
                notificationEnabled = false,
                onBehalfs = new List<string>() {
                    "admin"
                },
                priorityRole = "Manager",
                selectedOnBehalf = "admin",
                lastChanged = DateTime.UtcNow.Ticks,
                ownedCars = new List<Car>() {
                    new Car() {
                        batteryCapacity = 93000,
                        name = "Porsche Taycan",
                        connectors = new Dictionary<ConnectorType, double>() {
                            {
                                ConnectorType.CCS, 350000
                            }, {
                                ConnectorType.TYPE_TWO, 22000
                            }
                        }
                    }
                }
            }, config.GetValue<string>("adminPassword"));

            var self = this;
            persistor = new ManagerPersistor(() => {
                string jsonString;
                lock (self) {
                    var persistenceModel = new UserPersistenceModel() {
                        users = users, unverifiedUser = unverifiedUsers, passwordForgottenUser = passwordForgottenUsers
                    };
                    jsonString = JsonConvert.SerializeObject(persistenceModel, Formatting.Indented);
                }
                File.WriteAllText(pathToUsersFile, jsonString, Encoding.UTF8);
            });
        }

        private static UserPersistenceModel readUserFile(string path) {
            try {
                return JsonConvert.DeserializeObject<UserPersistenceModel>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (FileNotFoundException) {
                return new UserPersistenceModel() {
                    users = new Dictionary<string, User>(),
                    unverifiedUser = new Dictionary<string, UnverifiedUser>(),
                    passwordForgottenUser = new Dictionary<string, UserUrlToken>()
                };
            }
        }

        private static Dictionary<string, int> readPossiblePriorityRolesFile(string path) {
            try {
                return JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (FileNotFoundException) {
                return new Dictionary<string, int>();
            }
        }

        public override void changeSelectedOnBehalf(string email, string onBehalf) {
            base.changeSelectedOnBehalf(email, onBehalf);
            persistor.notifyChange();
        }

        public override bool changePassword(string email, string oldPassword, string newPassword) {
            var success = base.changePassword(email, oldPassword, newPassword);
            persistor.notifyChange();
            return success;
        }

        public override void addCar(string email, Car car) {
            base.addCar(email, car);
            persistor.notifyChange();
        }

        public override void deleteCar(string email, string carName) {
            base.deleteCar(email, carName);
            persistor.notifyChange();
        }

        public override string createUnverifiedUser(string email, bool isAdmin, string priorityRole) {
            var token = base.createUnverifiedUser(email, isAdmin, priorityRole);
            persistor.notifyChange();
            return token;
        }

        public override void registerUnverifiedUser(string email, string verificationToken, string password, string firstName, string lastName,
            string defaultLocation) {
            base.registerUnverifiedUser(email, verificationToken, password, firstName, lastName, defaultLocation);
            persistor.notifyChange();
        }

        public override string prepareForPasswordReset(string email) {
            var token = base.prepareForPasswordReset(email);
            persistor.notifyChange();
            return token;
        }

        public override void resetPassword(string email, string resetPasswordToken, string newPassword) {
            base.resetPassword(email, resetPasswordToken, newPassword);
            persistor.notifyChange();
        }

        public override void deleteVerifiedUser(string email) {
            base.deleteVerifiedUser(email);
            persistor.notifyChange();
        }

        public override void editUser(string email, string firstName, string lastName, string priorityRole, bool isAdmin) {
            base.editUser(email, firstName, lastName, priorityRole, isAdmin);
            persistor.notifyChange();
        }

        public override void deleteUnverifiedUser(string email) {
            base.deleteUnverifiedUser(email);
            persistor.notifyChange();
        }

        public override void setDefaultLocation(string email, string location) {
            base.setDefaultLocation(email, location);
            persistor.notifyChange();
        }

        public override void setNotificationEnabled(string email, bool enabled) {
            base.setNotificationEnabled(email, enabled);
            persistor.notifyChange();
        }

        public void Dispose() {
            persistor.Dispose();
        }

        private class UserPersistenceModel {
            public Dictionary<string, User> users;
            public Dictionary<string, UnverifiedUser> unverifiedUser;
            public Dictionary<string, UserUrlToken> passwordForgottenUser;
        }
    }
}