using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using chargebook.data.user;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using ChargeBook.services;
using System;
using chargebook.viewModels;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChargeBook.controllers {
    [Authorize(Policy = "AdminRequired")]
    public class HomeAdminController : Controller {
        private readonly IInfrastructureManager infrastructureManager;
        private readonly IBookingManager bookingManager;
        private readonly IUserManager userManager;

        public HomeAdminController(IInfrastructureManager infrastructureManager, IBookingManager bookingManager, IUserManager userManager) {
            this.infrastructureManager = infrastructureManager;
            this.bookingManager = bookingManager;
            this.userManager = userManager;
        }

        public IActionResult index() {
            ViewData["locations"] = infrastructureManager.getLocationNames();
            return View();
        }

        public IActionResult userAdmin() {
            ViewData["locations"] = infrastructureManager.getLocationNames();
            ViewData["possiblePriorityRoles"] = new List<string>(userManager.possiblePriorityRoles.Keys);
            return View();
        }

        public IActionResult bookingList() {
            return View(bookingManager.getAllBookings());
        }

        public IActionResult simulationHistory() {
            return View();
        }

        public IActionResult simulationLog() {
            return View();
        }
    }
}