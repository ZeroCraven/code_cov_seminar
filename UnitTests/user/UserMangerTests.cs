using System;
using System.Collections.Generic;
using System.Reflection;
using chargebook.data.user;
using chargebook.models;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;

namespace UnitTests.user {
    public class UserMangerTests {
        private IUserManager userManager;
        private User testUser;

        public UserMangerTests() {
            testUser = new User() {
                email = "testmail",
                defaultLocation = "Augsburg",
                firstName = "firstName",
                isAdmin = true,
                lastChanged = DateTime.UtcNow.Ticks,
                lastName = "lastName",
                notificationEnabled = true,
                onBehalfs = new List<string>() {
                    "testmail"
                },
                ownedCars = new List<Car>(),
                priorityRole = "Senior",
                selectedOnBehalf = "testmail"
            };
        }

        [SetUp]
        public void setUp() {
            userManager = new UserManager(new Dictionary<string, int>() {
                {
                    "Senior", 1
                }, {
                    "Junior", 0
                }
            });
        }

        [Test]
        public void testGenerateSalt() {
            MethodInfo generateSaltMethod = userManager.GetType().GetMethod("generateSalt",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var salt = generateSaltMethod.Invoke(userManager, new object[] { });
            Assert.IsTrue(salt is byte[]);
        }

        [Test]
        public void testHashPassword() {
            //generate salt first
            var generateSaltMethod = userManager.GetType().GetMethod("generateSalt",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var salt = generateSaltMethod.Invoke(userManager, new object[] { });
            //then test hash Password
            const string password = "testPassword";
            var hashPasswordMethod = userManager.GetType().GetMethod("hashPassword",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var passwordHash = hashPasswordMethod.Invoke(userManager, new object[] {
                password, salt
            });
            Assert.IsFalse(string.IsNullOrEmpty(passwordHash as string));
        }

        [Test]
        public void testEnsureUserExitsDoesAddUserToDictionary() {
            userManager.ensureUserExists(testUser, "test");
            var usersField = userManager.GetType().GetField("users",
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            var users = usersField.GetValue(userManager) as Dictionary<string, User>;
            Assert.IsTrue(users.ContainsKey("testmail"));
        }

        [Test]
        public void testEnsureUserExitsDoesHandlePasswordRight() {
            userManager.ensureUserExists(testUser, "test");

            var hashPasswordMethod = userManager.GetType().GetMethod("hashPassword",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var passwordHash = hashPasswordMethod.Invoke(userManager, new object[] {
                "test", testUser.salt,
            });
            Assert.AreEqual(passwordHash, testUser.password);
        }

        [Test]
        public void testFindIdentityPositive() {
            userManager.ensureUserExists(testUser, "test");
            Assert.NotNull(userManager.findIdentity(testUser.email, "test"));
        }

        public void testFindIdentityNegativeWithWrongPassword() {
            userManager.ensureUserExists(testUser, "test");
            Assert.IsNull(userManager.findIdentity(testUser.email, "testasdf"));
        }

        public void testFindIdentityNegativeWithWrongEmail() {
            userManager.ensureUserExists(testUser, "test");
            Assert.IsNull(userManager.findIdentity("aNotExistingEmail", "andSomePassword"));
        }

        public void testGetIdentityPositive() {
            userManager.ensureUserExists(testUser, "test");
            Assert.NotNull(userManager.getIdentity(testUser.email));
        }

        public void testGetIdentityNegativeWithWrongEmail() {
            userManager.ensureUserExists(testUser, "test");
            Assert.IsNull(userManager.getIdentity("aNotExistingEmail"));
        }

        public void testDoesLastChangedMatchPositive() {
            userManager.ensureUserExists(testUser, "test");
            Assert.IsTrue(userManager.isUpToDate(testUser.email, testUser.lastChanged));
        }

        public void testDoesLastChangedMatchNegativeWithDifferentLastChanged() {
            userManager.ensureUserExists(testUser, "test");
            Assert.IsFalse(userManager.isUpToDate(testUser.email, testUser.lastChanged - 234));
        }

        [Test]
        public void testUserRegistration() {
            var token = userManager.createUnverifiedUser("test@mail.de", true, "Senior");
            userManager.registerUnverifiedUser("test@mail.de", token, "test", "firstName", "lastName", "Augsburg");
            Assert.IsNotNull(userManager.findIdentity("test@mail.de", "test"));
        }

        public void testUserRegistrationWithWrongToken() {
            var token = userManager.createUnverifiedUser("test@mail.de", true, "Senior");
            Assert.Throws<ValidTokenEmailCombinationNotFoundException>(() => {
                userManager.registerUnverifiedUser("test@mail.de", token + "someErrorString", "test", "firstName", "lastName", "Augsburg");
            });
            Assert.IsNull(userManager.findIdentity("test@mail.de", "test"));
        }

        public void testUserRegistrationOnAlreadyVerifiedUser() {
            userManager.ensureUserExists(testUser, "test");
            Assert.Throws<UserAlreadyExistsException>(() => {
                userManager.createUnverifiedUser(testUser.email, true, "Senior");
            });
        }

        [Test]
        public void testUserPasswordForgottenProcess() {
            userManager.ensureUserExists(testUser, "forgottenPassword");
            var token = userManager.prepareForPasswordReset(testUser.email);
            userManager.resetPassword(testUser.email, token, "newPassword");
            Assert.IsNotNull(userManager.findIdentity(testUser.email, "newPassword"));
        }

        [Test]
        public void testUserPasswordForgottenProcessWithWrongToken() {
            userManager.ensureUserExists(testUser, "forgottenPassword");
            var token = userManager.prepareForPasswordReset(testUser.email);
            Assert.Throws<ValidTokenEmailCombinationNotFoundException>(() => {
                userManager.resetPassword(testUser.email, token + "someErrorString", "test");
                Assert.IsNull(userManager.findIdentity(testUser.email, "test"));
            });
        }

        [Test]
        public void testPrepareForPasswordResetWithNotExistingUser() {
            Assert.Throws<UserNotFoundException>(() => userManager.prepareForPasswordReset("test@mail.de"));
        }

        [Test]
        public void testCheckUnverifiedUserVerificationToken() {
            var token = userManager.createUnverifiedUser("test@mail.de", true, "Senior");
            Assert.IsTrue(userManager.checkUnverifiedUserVerificationToken("test@mail.de", token));
        }

        [Test]
        public void testCheckUnverifiedUserVerificationTokenNegative() {
            var token = userManager.createUnverifiedUser("test@mail.de", true, "Senior");
            Assert.IsFalse(userManager.checkUnverifiedUserVerificationToken("test@mail.de", token + "someError"));
        }

        public void testCheckUnverifiedUserVerificationTokenWithNotExistingUser() {
            Assert.IsFalse(userManager.checkUnverifiedUserVerificationToken("notExistingUserEmail", "someToken"));
        }

        [Test]
        public void testCheckResetPasswordToken() {
            userManager.ensureUserExists(testUser, "test");
            var token = userManager.prepareForPasswordReset(testUser.email);
            Assert.IsTrue(userManager.checkResetPasswordToken(testUser.email, token));
        }

        [Test]
        public void testCheckResetPasswordTokenNegative() {
            userManager.ensureUserExists(testUser, "test");
            var token = userManager.prepareForPasswordReset(testUser.email);
            Assert.IsFalse(userManager.checkResetPasswordToken(testUser.email, token + "someError"));
        }

        [Test]
        public void testCheckResetPasswordTokenWithNotExistingUser() {
            Assert.IsFalse( userManager.checkResetPasswordToken("notExisting@User.de", "someToken"));
        }
    }
}