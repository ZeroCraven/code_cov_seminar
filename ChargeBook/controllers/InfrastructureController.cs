using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using chargebook.data.user;
using chargebook.models.infrastructure;
using chargebook.viewModels.infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChargeBook.controllers {
    [Authorize(Policy = "AdminRequired")]
    public class InfrastructureController : Controller {
        private readonly IInfrastructureManager infrastructureManager;

        public InfrastructureController(IInfrastructureManager infrastructureManager) {
            this.infrastructureManager = infrastructureManager;
        }

        public IActionResult index(LocationViewModel viewModel) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("locations");
            }
            ViewData["locationName"] = viewModel.location;
            ViewData["chargeStationTypes"] = new List<string>(infrastructureManager.possibleChargeStationTypes.Keys);
            return View(infrastructureManager.getInfrastructureByLocation(viewModel.location));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult createLocation(CreateLocationViewModel viewModel) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("locations");
            }
            try {
                infrastructureManager.createLocation(viewModel.location,
                    TimeZoneInfo.GetSystemTimeZones().First(timeZoneInfo => timeZoneInfo.Id == viewModel.timeZoneId));
                return RedirectToAction("locations", "Infrastructure", new {
                    viewModel.location
                });
            }
            catch (InfrastructurePartAlreadyExistsException e) {
                TempData["errorMessage"] = e.Message;
                return RedirectToAction("locations");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult deleteLocation(LocationViewModel viewModel, [FromServices] IBookingManager bookingManager,
            [FromServices] IUserManager userManager) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("locations");
            }
            infrastructureManager.deleteLocation(viewModel.location);
            bookingManager.deleteBookingsFromLocation(viewModel.location);
            userManager.handleLocationDeletion(viewModel.location);
            return RedirectToAction("locations", "Infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult createChargeGroup(CreateChargeGroupViewModel viewModel) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("index", new {
                    viewModel.location
                });
            }
            try {
                infrastructureManager.createChargeGroup(viewModel.location, viewModel.chargeGroupName, viewModel.maxChargePower);
            }
            catch (InfrastructurePartAlreadyExistsException e) {
                TempData["errorMessage"] = e.Message;
            }
            return RedirectToAction("index", "Infrastructure", new {
                viewModel.location
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult deleteChargeGroup(DeleteChargeGroupViewModel viewModel, [FromServices] IBookingManager bookingManager) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("index", new {
                    viewModel.location
                });
            }
            try {
                infrastructureManager.deleteChargeGroup(viewModel.location, viewModel.chargeGroupName);
                bookingManager.deleteBookingsFromChargeGroup(viewModel.location, viewModel.chargeGroupName);
            }
            catch (InfrastructurePartNotFoundException e) {
                TempData["errorMessage"] = e.Message;
            }


            return RedirectToAction("index", "Infrastructure", new {
                viewModel.location
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult createChargeStation(CreateChargeStationViewModel viewModel) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("index", new {
                    viewModel.location
                });
            }
            try {
                infrastructureManager.createChargeStation(viewModel.location, viewModel.chargeGroupName, viewModel.chargeStationName,
                    viewModel.chargeStationTypeName);
            }
            catch (InfrastructurePartAlreadyExistsException e) {
                TempData["errorMessage"] = e.Message;
            }
            catch (InfrastructurePartNotFoundException e) {
                TempData["errorMessage"] = e.Message;
            }
            return RedirectToAction("index", "Infrastructure", new {
                viewModel.location
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult deleteChargeStation(DeleteChargeStationViewModel viewModel, [FromServices] IBookingManager bookingManager) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("index", new {
                    viewModel.location
                });
            }
            try {
                infrastructureManager.deleteChargeStation(viewModel.location, viewModel.chargeGroupName, viewModel.chargeStationName);
                bookingManager.deleteBookingsFromChargeStation(viewModel.location, viewModel.chargeGroupName, viewModel.chargeStationName);
            }
            catch (InfrastructurePartAlreadyExistsException e) {
                TempData["errorMessage"] = e.Message;
            }
            catch (InfrastructurePartNotFoundException e) {
                TempData["errorMessage"] = e.Message;
            }
            return RedirectToAction("index", "Infrastructure", new {
                viewModel.location
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult setInfrastructureSettings(InfrastructureSettingsViewModel viewModel) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("index", new LocationViewModel() {
                    location = viewModel.location,
                });
            }
            infrastructureManager.setInfrastructureSettings(viewModel.location, viewModel.beginBuffer, viewModel.endBuffer,
                viewModel.minReservedCCSConnectors, viewModel.minReservedChademo, viewModel.minReservedType2);
            return RedirectToAction("index", new LocationViewModel() {
                location = viewModel.location,
            });
        }

        public IActionResult locations() {
            ViewData["locations"] = infrastructureManager.getLocationNames();
            return View();
        }

        public IActionResult infrastructure([Required(ErrorMessage = "Der Standort ist ein Pflichfeld")]
            string locationName) {
            if (string.IsNullOrEmpty(locationName)) {
                return BadRequest();
            }
            Infrastructure infrastructure;
            try {
                infrastructure = infrastructureManager.getInfrastructureByLocation(locationName);
            }
            catch (InfrastructurePartNotFoundException e) {
                return BadRequest(e.Message);
            }
            return Ok(JsonConvert.SerializeObject(new {
                infrastructure = infrastructure
            }));
        }

    }
}