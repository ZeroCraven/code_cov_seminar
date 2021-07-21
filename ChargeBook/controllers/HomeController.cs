using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChargeBook.data.booking;
using chargebook.data.user;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using chargebook.models;
using ChargeBook.services;
using Microsoft.AspNetCore.Authorization;

namespace chargebook.controllers {
    [Authorize(Policy = "LoggedInRequired")]
    public class HomeController : Controller {
        private IBookingManager bookingManager;

        public HomeController(IBookingManager bookingManager) {
            this.bookingManager = bookingManager;
        }

        public IActionResult index([FromServices] IUserUtils userUtils) {
            return View(bookingManager.getBookingsByUserEmail(userUtils.getEmail(User)));
        }

        public IActionResult privacy() {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult error() {
            return View(new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}