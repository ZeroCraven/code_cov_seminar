using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ChargeBook.attributes.validationAttributes;
using ChargeBook.attributes.validationAttributes.AdminCreateBookingViewModel;
using ChargeBook.attributes.validationAttributes.simulation;
using chargebook.data.infrastructure;
using chargebook.data.user;
using chargebook.models;
using ChargeBook.models.booking;
using chargebook.models.infrastructure;
using ChargeBook.models.simulation;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.Cms;

namespace UnitTests {
    public class ValidationAttributesTest {

        private class LocationTestViewModel {
            [LocationExistsValidation] public string location { get; set; }
        }

        private class CarTestViewModel {
            public string email { get; set; }
            [CarExistsValidation(nameof(email))] public string car { get; set; }
        }

        private class GreaterOrLowerTestViewModel {
            [CompareGreaterOrLowerValidation(nameof(value2), false)]
            public int value1 { get; set; }

            public int value2 { get; set; }
        }

        private class TimePeriodTestViewModel {
            [TimePeriodInFutureValidation]
            [TimePeriodPositiveTimeSpanValidation]
            public TimePeriod timePeriod { get; set; }
        }

        private class StatusTestViewModel {
            [StatusValidation]
            public string status { get; set; }
        }

        private class TimePeriodMaxValueValidationViewModel {
            [TimePeriodMaxValueValidation]
            public TimePeriod timePeriod { get; set; }
        }

        private class TimePeriodMinutesMultiplesOfValidationViewModel {
            [TimePeriodMinutesMultiplesOfValidation(60)]
            public TimePeriod timePeriod { get; set; }            
            
        }

        private class ConnectorListValidationViewModel{
            [ConnectorListValidation] public Dictionary<ConnectorType, double> dictionary { get; set; }
        }
        

        private class TimeZoneInfoTestViewModel {
            [TimeZoneInfoFormatValidation] public string timeZoneInfo { get; set; }
        }


        private class MultiplesOfTestViewModel {
            [MultiplesOfValidation(15)] public int x { get; set; }
        }

        private class MyProvider : IServiceProvider {
            public object GetService(Type serviceType) {
                if (serviceType == typeof(IInfrastructureManager)) {
                    InfrastructureManager infrastructureManager = new InfrastructureManager(new Dictionary<string, ChargeStationType>());
                    infrastructureManager.createLocation("Augsburg", TimeZoneInfo.Local);
                    return infrastructureManager;
                }
                if (serviceType == typeof(IUserManager)) {
                    var userManager = new UserManager(new Dictionary<string, int>());
                    var userManagerField = userManager.GetType().GetField("users",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var users = userManagerField.GetValue(userManager) as Dictionary<string, User>;
                    users.Add("testMail",new User() {
                        email = "testMail",
                    });
                    userManager.addCar("testMail",new Car() {
                        name = "bmwi3"
                    });
                    return userManager;
                }
                return null;
            }
        }

        private LocationTestViewModel locationViewModel1;
        private LocationTestViewModel locationViewModel2;
        private CarTestViewModel carViewModel1;
        private CarTestViewModel carViewModel2;
        private GreaterOrLowerTestViewModel greaterOrLowerTestViewModel;
        private TimePeriodTestViewModel timePeriodTestViewModel;
        private TimeZoneInfoTestViewModel timeZoneInfoTestViewModel;
        private MultiplesOfTestViewModel multiplesOfTestViewModel;
        private StatusTestViewModel statusTestViewModel;
        private TimePeriodMinutesMultiplesOfValidationViewModel timePeriodMinutesMultiplesOfValidationViewModel;
        private TimePeriodMaxValueValidationViewModel timePeriodMaxValueValueValidationViewModel;
        private ConnectorListValidationViewModel connectorListValidationViewModel; 

        [OneTimeSetUp]
        public void setup() {
            locationViewModel1 = new LocationTestViewModel() {
                location = "Augsburg",
            };
            locationViewModel2 = new LocationTestViewModel() {
                location = "München",
            };
            carViewModel1 = new CarTestViewModel() {
                car = "bmwi3",
                email = "testMail"
            };
            carViewModel2 = new CarTestViewModel() {
                car = "tesla3",
                email = "testMail"
            };
            greaterOrLowerTestViewModel = new GreaterOrLowerTestViewModel() {
                value1 = 2, value2 = 100,
            };
            timePeriodTestViewModel = new TimePeriodTestViewModel() {
                timePeriod = new TimePeriod(DateTime.Now.AddYears(50), DateTime.Now.AddYears(60)),
            };
            timeZoneInfoTestViewModel = new TimeZoneInfoTestViewModel() {
                timeZoneInfo = TimeZoneInfo.Utc.Id
            };
            multiplesOfTestViewModel = new MultiplesOfTestViewModel() {
                x = 45,
            };
            statusTestViewModel = new StatusTestViewModel() {
                status = "angenommen"
            };
            timePeriodMinutesMultiplesOfValidationViewModel = new TimePeriodMinutesMultiplesOfValidationViewModel() {
                timePeriod = new TimePeriod(DateTime.Now.Date.AddHours(3),DateTime.Now.Date.AddHours(4))
            };
            timePeriodMaxValueValueValidationViewModel = new TimePeriodMaxValueValidationViewModel() {
                timePeriod = new TimePeriod(DateTime.Now.Date.AddHours(3),DateTime.Now.Date.AddHours(4))
            };
            Dictionary<ConnectorType, double> dict = new Dictionary<ConnectorType, double>();
            dict.Add(ConnectorType.CCS,500);
            connectorListValidationViewModel = new ConnectorListValidationViewModel() {
                dictionary = dict,
            };
        }

        [Test]
        public void locationValidationAttributeTest() {
            var context = new ValidationContext(locationViewModel1, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(locationViewModel1, context, results, true);
            Assert.True(isValid);

            context = new ValidationContext(locationViewModel2, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(locationViewModel2, context, results, true);
            Assert.False(isValid);
        }

        [Test]
        public void carValidationAttributeTest() {
            var context = new ValidationContext(carViewModel1, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(carViewModel1, context, results, true);
            Assert.True(isValid);
            context = new ValidationContext(carViewModel2, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(carViewModel2, context, results, true);
            Assert.False(isValid);
        }

        [Test]
        public void greaterOrLowerValidationAttributeTest() {
           
            var context = new ValidationContext(greaterOrLowerTestViewModel, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(greaterOrLowerTestViewModel, context, results, true);
            Assert.True(isValid);
            greaterOrLowerTestViewModel.value1 = 3;
            greaterOrLowerTestViewModel.value2 = 2;
            context = new ValidationContext(greaterOrLowerTestViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(greaterOrLowerTestViewModel, context, results, true);
            Assert.False(isValid);
           
        }

        [Test]
        public void timePeriodValidationAttributeTest() {
            
            var context = new ValidationContext(timePeriodTestViewModel, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(timePeriodTestViewModel, context, results, true);
            Assert.True(isValid);
            timePeriodTestViewModel.timePeriod.startTime = DateTime.Now.AddDays(-10);
            timePeriodTestViewModel.timePeriod.endTime = DateTime.Now;
            context = new ValidationContext(timePeriodTestViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(timePeriodTestViewModel, context, results, true);
            Assert.False(isValid);
            timePeriodTestViewModel.timePeriod.startTime = DateTime.Now.AddYears(10);
            timePeriodTestViewModel.timePeriod.endTime = DateTime.Now.AddYears(5);
            context = new ValidationContext(timePeriodTestViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(timePeriodTestViewModel, context, results, true);
            Assert.False(isValid);
            
        }

        [Test]
        public void timeZoneInfoValidationAttributeTest() {
            var context = new ValidationContext(timeZoneInfoTestViewModel, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(timeZoneInfoTestViewModel, context, results, true);
            Assert.True(isValid);
            timeZoneInfoTestViewModel.timeZoneInfo = "wrongString";
            context = new ValidationContext(timeZoneInfoTestViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(timeZoneInfoTestViewModel, context, results, true);
            Assert.False(isValid);
        }
        

        [Test]
        public void multiplesOfValidationAttributeTest() {
            var context = new ValidationContext(multiplesOfTestViewModel, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(multiplesOfTestViewModel, context, results, true);
            Assert.True(isValid);
            multiplesOfTestViewModel.x = 17;
            context = new ValidationContext(multiplesOfTestViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(multiplesOfTestViewModel, context, results, true);
            Assert.False(isValid);
        }

        [Test]
        public void statusValidationAttributeTest() {
            var context = new ValidationContext(statusTestViewModel, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(statusTestViewModel, context, results, true);
            Assert.True(isValid);
            statusTestViewModel.status = "quatsch";
            context = new ValidationContext(statusTestViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(statusTestViewModel, context, results, true);
            Assert.False(isValid);
        }
        [Test]
        public void timePeriodMinutesMultiplesOfValidationAttributeTest() {
            var context = new ValidationContext(timePeriodMinutesMultiplesOfValidationViewModel, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(timePeriodMinutesMultiplesOfValidationViewModel, context, results, true);
            Assert.True(isValid);
            timePeriodMinutesMultiplesOfValidationViewModel.timePeriod = new TimePeriod(DateTime.Now.Date.AddMinutes(12), DateTime.Now.AddMinutes(17));
            context = new ValidationContext(timePeriodMinutesMultiplesOfValidationViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(timePeriodMinutesMultiplesOfValidationViewModel, context, results, true);
            Assert.False(isValid);
        }

        [Test]
        public void timePeriodMaxValueValidationTest() {
            var context = new ValidationContext(timePeriodMaxValueValueValidationViewModel, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(timePeriodMaxValueValueValidationViewModel, context, results, true);
            Assert.True(isValid);
            timePeriodMaxValueValueValidationViewModel.timePeriod = new TimePeriod(DateTime.Now.Date.AddMinutes(12), DateTime.Now.AddDays(50));
            context = new ValidationContext(timePeriodMaxValueValueValidationViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(timePeriodMaxValueValueValidationViewModel, context, results, true);
            Assert.False(isValid);
        }

        [Test]

        public void connectorListValidationTest() {
            var context = new ValidationContext(connectorListValidationViewModel, new MyProvider(), null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(connectorListValidationViewModel, context, results, true);
            Assert.True(isValid);
            connectorListValidationViewModel.dictionary.Add(ConnectorType.TYPE_ONE,-200);
            context = new ValidationContext(connectorListValidationViewModel, new MyProvider(), null);
            results = new List<ValidationResult>();
            isValid = Validator.TryValidateObject(connectorListValidationViewModel, context, results, true);
            Assert.False(isValid);
        }




    }

}