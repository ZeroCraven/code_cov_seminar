﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Edge.SeleniumTools;

namespace UITest {
    [TestFixture("Chrome")]
    [TestFixture("Edge")]
    public class DeleteBookingUITest {

        private IWebDriver driver;
        private string browser;
        private WebDriverWait webDriverWait;

        public DeleteBookingUITest(string testBrowser) {
            browser = testBrowser;
        }

        [OneTimeSetUp]
        public void initialize() {
            if (browser == "Edge") {
                EdgeOptions edgeOptions = new EdgeOptions();
                edgeOptions.UseChromium = true;
                driver = new EdgeDriver(edgeOptions);
            } else if (browser == "Chrome") {
                driver = new ChromeDriver();
            }
            webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            login();
        }

        public void login() {
            driver.Navigate().GoToUrl("https://localhost:5001/user/login");
            webDriverWait.Until(driver => driver.FindElement(By.Id("loginSubmit")));
            IWebElement loginName = driver.FindElement(By.Id("email"));
            IWebElement loginPassword = driver.FindElement(By.Id("password"));
            IWebElement loginButton = driver.FindElement(By.Id("loginSubmit"));

            loginName.Clear();
            loginPassword.Clear();
            
            // Need to adjusted, if this account isn't excisiting anymore in the system
            loginName.SendKeys("admin");
            loginPassword.SendKeys("chargeb00k");
            loginButton.Submit();
        }

        // If the Delete Method don't find any excisting Booking in the Home Index, this method will create a new Booking Request for Deletion
        // Method counts the Bookings for the User and delete one Booking -> Compares the Counter before and after the Delete for Boolean Answer
        // -> Expected Result: Success
        [TestCase(0, 0, 1, 99)]
        public void testDeleteBooking(int indexLocation, int indexCar, int valueStartSoC, int valueTargetSoC) {
            IEnumerable<IWebElement> bookingListRows = driver.FindElements(By.CssSelector(".booking-list-row"));
            int bookingCountOld = bookingListRows.Count();

            if (bookingCountOld == 0) {
                // Go to Create Booking Request View
                webDriverWait.Until(driver => driver.FindElement(By.Id("createBookingRequest")));
                webDriverWait.Until<bool>(driver => {
                    try {
                        driver.FindElement(By.Id("createBookingRequest")).Click();
                        return false;
                    }
                    catch (NotFoundException) {
                        return true;
                    }
                });
                webDriverWait.Until(driver => driver.FindElement(By.Id("submitBooking")));
                Assert.IsTrue(driver.Url.EndsWith("Booking/create"));

                // Select Elements of the View
                IWebElement dropdownSelectLocations = driver.FindElement(By.Id("selectLocation"));
                IWebElement dropdownSelectCar = driver.FindElement(By.Id("selectCar"));
                IWebElement inputStartSoC = driver.FindElement(By.Id("startSoC"));
                IWebElement inputTargetSoC = driver.FindElement(By.Id("targetSoC"));
                IWebElement inputTimePeriodStart = driver.FindElement(By.Name("timePeriods[0].startTime"));
                IWebElement inputTimePeriodEnd = driver.FindElement(By.Name("timePeriods[0].endTime"));
                IWebElement buttonSubmitBooking = driver.FindElement(By.Id("submitBooking"));
                IWebElement buttonBackToIndex = driver.FindElement(By.Id("cancelBooking"));
                SelectElement dropdownLocations = new SelectElement(dropdownSelectLocations);
                SelectElement dropdownCar = new SelectElement(dropdownSelectCar);

                // create start and end Times for the Date Time Picker of the View
                DateTime start = new DateTime();
                DateTime end = new DateTime();

                // set start and end
                start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                start = start.AddDays(2);
                end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                end = end.AddDays(2);
                end = end.AddHours(3);


                // Input for Location
                if (dropdownLocations.Options.Any()) {
                    dropdownLocations.SelectByIndex(indexLocation);
                }

                // Input for Car
                if (dropdownCar.Options.Any()) {
                    dropdownCar.SelectByIndex(indexCar);
                }

                // Input for StartSoc
                for (int i = 0; i < valueStartSoC; i++) {
                    inputStartSoC.SendKeys(Keys.Right);
                }

                // Input for EndSoc
                for (int i = 0; i < valueTargetSoC; i++) {
                    inputTargetSoC.SendKeys(Keys.Right);
                }

                // Fill TimePeriod with Starttime
                inputTimePeriodStart.SendKeys(start.ToString("dd") + start.ToString("MM") + start.ToString("yyyy"));
                inputTimePeriodStart.SendKeys(Keys.Right);
                inputTimePeriodStart.SendKeys(start.ToString("HH") + start.ToString("mm"));

                // Fill TimePeriod with Endtime
                inputTimePeriodEnd.SendKeys(end.ToString("dd") + end.ToString("MM") + end.ToString("yyyy"));
                inputTimePeriodEnd.SendKeys(Keys.Right);
                inputTimePeriodEnd.SendKeys(end.ToString("HH") + end.ToString("mm"));

                // Submit Formular of the Booking Request View
                buttonSubmitBooking.Submit();

                // Looking for Error Message
                if (!dropdownLocations.Options.Any()) {
                    Assert.DoesNotThrow(() => driver.FindElement(By.Id("errorMessage")));
                } else if (!dropdownCar.Options.Any()) {
                    Assert.DoesNotThrow(() => driver.FindElement(By.Id("errorMessage")));
                }

                // go back to Home Index View
                buttonBackToIndex.Click();
            }
            bookingListRows = driver.FindElements(By.CssSelector(".booking-list-row"));
            bookingCountOld = bookingListRows.Count();
            IWebElement buttonDetails = driver.FindElement(By.CssSelector(".booking-list-row td:nth-child(6) button"));
            webDriverWait.Until<bool>(driver => {
                try {
                    buttonDetails.Click();
                    return false;
                }
                catch (Exception) {
                    return true;
                }
            });
            webDriverWait.Until(driver => driver.FindElement(By.Id("cancel-btn")));
            IWebElement deleteButtonBooking = driver.FindElement(By.Id("cancel-btn"));
            deleteButtonBooking.Click();
            bookingListRows = driver.FindElements(By.CssSelector(".booking-list-row"));
            int bookingCountNew = bookingListRows.Count();
            webDriverWait.Until<bool>(driver => {
                bookingListRows = driver.FindElements(By.CssSelector(".booking-list-row"));
                bookingCountNew = bookingListRows.Count();
                return (bookingCountNew != bookingCountOld);
            });
            Assert.IsTrue(bookingCountNew == (bookingCountOld - 1));
        }
        
        [OneTimeTearDown]
        public void close() {
            driver.Quit();
        }
    }
}
