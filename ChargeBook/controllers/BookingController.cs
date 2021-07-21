using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using chargebook.data.user;
using Microsoft.AspNetCore.Mvc;
using chargebook.models;
using ChargeBook.models.booking;
using ChargeBook.models.booking.bookingExceptions;
using chargebook.viewModels;
using chargebook.models.infrastructure;
using ChargeBook.services;
using ChargeBook.viewModels.bookingViewModels;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace ChargeBook.controllers {
    [Authorize(Policy = "LoggedInRequired")]
    public class BookingController : Controller {
        private readonly IBookingManager bookingManager;
        private readonly IUserUtils userUtils;

        public BookingController(IBookingManager bookingManager, IUserUtils userUtils) {
            this.bookingManager = bookingManager;
            this.userUtils = userUtils;
        }

        [HttpGet]
        public IActionResult create() {
            ViewData["isAdminView"] = false;
            return View();
        }

        [HttpGet]
        public IActionResult currentBookings() {
            return Ok(JsonConvert.SerializeObject(bookingManager.getCurrentBookingsByUser(userUtils.getEmail(User))));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult create([FromServices] IUserManager userManager, RequestedBookingUserViewModel requestedBookingUserViewModel,
            [FromServices] IInfrastructureManager infrastructureManager) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }
            string mail = userUtils.getEmail(User);
            Car car;
            try {
                car = userUtils.getCars(User).Find(x => x.name == requestedBookingUserViewModel.carName);
            }
            catch (ArgumentNullException) {
                return BadRequest("Es wurde kein passendes Auto mit dem Namen '"+requestedBookingUserViewModel.carName+"' gefunden");
            }
            if (car == null) {
                return BadRequest("Es wurde kein Auto ausgewählt");
            }
            TimeZoneInfo timezoneinfo = infrastructureManager.getInfrastructureByLocation(requestedBookingUserViewModel.location).timeZone;

            List<TimePeriod> adHocTimePeriods = requestedBookingUserViewModel.timePeriods.Where(x => {
                DateTime convertedStartTime = TimeZoneInfo.ConvertTimeFromUtc(x.startTime, timezoneinfo);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezoneinfo).Day == convertedStartTime.Day;
            }).ToList();

            List<TimePeriod> timePeriods = requestedBookingUserViewModel.timePeriods.Where(x => {
                DateTime convertedStartTime = TimeZoneInfo.ConvertTimeFromUtc(x.startTime, timezoneinfo);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezoneinfo).Day != convertedStartTime.Day;
            }).ToList();

            if (adHocTimePeriods.Count > 0) {
                int id = bookingManager.createRequestedBooking(mail, requestedBookingUserViewModel.location
                    , car, (double) requestedBookingUserViewModel.startSoC / 100, (double) requestedBookingUserViewModel.targetSoC / 100,
                    adHocTimePeriods, userManager.getPriority(mail));
                RequestedBooking rb;
                try {
                    rb = (RequestedBooking) bookingManager.getBookingById(id);
                }
                catch (BookingNotFoundException) {
                    return BadRequest();
                }
                BookingDistributor.distributeAdHoc(id, rb, infrastructureManager.getInfrastructureByLocation(requestedBookingUserViewModel.location),
                    bookingManager,
                    requestedBookingUserViewModel.location);
                Booking adHocBooking;
                try {
                    adHocBooking = bookingManager.getBookingById(id);
                }
                catch (BookingNotFoundException) {
                    return BadRequest();
                }
                if (adHocBooking is ApprovedBooking) {
                    return Ok();
                }
            }
            if (timePeriods.Count > 0) {
                bookingManager.createRequestedBooking(mail, requestedBookingUserViewModel.location
                    , car, (double) requestedBookingUserViewModel.startSoC / 100, (double) requestedBookingUserViewModel.targetSoC / 100,
                    timePeriods, userManager.getPriority(mail));
            }
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult delete(int bookingId) {
            if (!isIdOfOwnBooking(bookingId)) {
                return BadRequest("Sie sind nicht berechtigt auf diese Buchung zuzugreifen");
            }
            bookingManager.deleteBooking(bookingId);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult editRequestedBookingUser(int bookingId, RequestedBookingUserViewModel requestedBooking,
            [FromServices] IUserManager userManager) {
            if (!isIdOfOwnBooking(bookingId)) {
                return BadRequest("Sie sind nicht berechtigt auf diese Buchung zuzugreifen");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            string mail = userUtils.getEmail(User);
            Car car;
            try {
                car = userUtils.getCars(User).Find(x => x.name == requestedBooking.carName);
            }
            catch (ArgumentNullException) {
                return BadRequest();
            }
            try {
                if (bookingManager.getBookingById(bookingId) is RequestedBooking rb) {
                    bookingManager.editRequestedBooking(bookingId, rb.denied, userManager.getPriority(mail), location: requestedBooking.location,
                        car: car,
                        startSoC: (double) requestedBooking.startSoC / 100, targetSoC: (double) requestedBooking.targetSoC / 100,
                        timePeriods: requestedBooking.timePeriods);
                }
            }
            catch (BookingNotFoundException) {
                return BadRequest();
            }
            return RedirectToAction("index", "home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult confirmChargeBegin(int bookingId) {
            if (!isIdOfOwnBooking(bookingId)) {
                return BadRequest("Sie sind nicht berechtigt auf diese Buchung zuzugreifen");
            }
            try {
                bookingManager.confirmChargeBegin(bookingId);
            }
            catch (BookingNotFoundException) {
                return BadRequest();
            }
            catch (WrongBookingTypeException) {
                return BadRequest();
            }
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult confirmChargeEnd(int bookingId) {
            if (!isIdOfOwnBooking(bookingId)) {
                return BadRequest("Sie sind nicht berechtigt auf diese Buchung zuzugreifen");
            }
            try {
                bookingManager.confirmChargeEnd(bookingId);
            }
            catch (BookingNotFoundException) {
                return BadRequest();
            }
            catch (WrongBookingTypeException) {
                return BadRequest();
            }
            return Ok();
        }

        private bool isIdOfOwnBooking(int bookingId) {
            string email = userUtils.getEmail(User);
            return bookingManager.isUserEntitledToUseBookingId(bookingId, email) || userUtils.isAdmin(User);
        }

        [HttpGet]
        public IActionResult bookingDetails(int bookingId) {
            if (!isIdOfOwnBooking(bookingId)) {
                return BadRequest("Sie sind nicht berechtigt auf diese Buchung zuzugreifen");
            }
            try {
                var booking = bookingManager.getBookingById(bookingId);
                return Ok(JsonConvert.SerializeObject(BookingsViewModel.fromBooking(bookingId, booking)));
            }
            catch (BookingNotFoundException) {
                return BadRequest();
            }
        }

        [HttpGet]
        public IActionResult list() {
            var bookings = bookingManager.getBookingsByUserEmail(userUtils.getEmail(User));
            var bookingViewModels = bookings.Select(pair => BookingsViewModel.fromBooking(pair.Key, pair.Value));
            return Ok(JsonConvert.SerializeObject(bookingViewModels));
        }

        [HttpGet]
        [Authorize(Policy = "AdminRequired")]
        public IActionResult listAll() {
            var bookings = bookingManager.getAllBookings();
            var bookingViewModels = bookings.Select(pair => BookingsViewModel.fromBooking(pair.Key, pair.Value));
            return Ok(JsonConvert.SerializeObject(bookingViewModels));
        }

        [HttpGet]
        [Authorize(Policy = "AdminRequired")]
        public IActionResult runDistributor() {
            return View();
        }
        
        [Authorize(Policy = "AdminRequired")]
        [HttpPost]
        public IActionResult runDistributor(string location, [FromServices] IInfrastructureManager infrastructureManager) {
            BookingDistributor.distribute(bookingManager, location, infrastructureManager.getInfrastructureByLocation(location));
            return Ok();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminRequired")]
        public IActionResult adminCreate(AdminCreateBookingViewModel viewModel, [FromServices] IInfrastructureManager infrastructureManager,
            [FromServices] IUserManager userManager) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }

            var userEmail = viewModel.user.ToLower();
            Car car;
            try {
                car = userManager.getCars(userEmail).First(x => x.name == viewModel.car);
            }
            catch (ArgumentNullException) {
                return BadRequest("Das angegebene Auto wurde im System nicht gefunden");
            }
            catch (InvalidOperationException) {
                return BadRequest("Das angegebene Auto existiert im System nicht mehr");
            }
            if (car == null) {
                return BadRequest("Es wurde kein Auto ausgewählt");
            }
            
            if (viewModel.status == "angefordert" || viewModel.status == "abgelehnt") {
                if (viewModel.status == "abgelehnt") {
                    bookingManager.createDeniedRequestedBooking(userEmail, viewModel.location
                        , car, (double) viewModel.startSoC / 100, (double) viewModel.targetSoC / 100,
                        viewModel.timePeriods);
                    return Ok();
                }
                TimeZoneInfo timezoneinfo = infrastructureManager.getInfrastructureByLocation(viewModel.location).timeZone;
                List<TimePeriod> adHocTimePeriods = viewModel.timePeriods.Where(x => {
                    DateTime convertedStartTime = TimeZoneInfo.ConvertTimeFromUtc(x.startTime, timezoneinfo);
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezoneinfo).Day == convertedStartTime.Day;
                }).ToList();
                List<TimePeriod> timePeriods = viewModel.timePeriods.Where(x => {
                    DateTime convertedStartTime = TimeZoneInfo.ConvertTimeFromUtc(x.startTime, timezoneinfo);
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezoneinfo).Day != convertedStartTime.Day;
                }).ToList();
                if (adHocTimePeriods.Count > 0) {
                    int id = bookingManager.createRequestedBooking(userEmail, viewModel.location
                        , car, (double) viewModel.startSoC / 100, (double) viewModel.targetSoC / 100,
                        adHocTimePeriods, userManager.getPriority(userEmail));
                    RequestedBooking rb;
                    try {
                        rb = (RequestedBooking) bookingManager.getBookingById(id);
                    }
                    catch (BookingNotFoundException) {
                        return BadRequest();
                    }
                    BookingDistributor.distributeAdHoc(id, rb, infrastructureManager.getInfrastructureByLocation(viewModel.location), bookingManager,
                        viewModel.location);
                    Booking adHocBooking;
                    try {
                        adHocBooking = bookingManager.getBookingById(id);
                    }
                    catch (BookingNotFoundException) {
                        return BadRequest();
                    }
                    if (adHocBooking is ApprovedBooking) {
                        return Ok();
                    }
                }
                if (timePeriods.Count > 0) {
                    bookingManager.createRequestedBooking(userEmail, viewModel.location
                        , car, (double) viewModel.startSoC / 100, (double) viewModel.targetSoC / 100,
                        timePeriods, userManager.getPriority(userEmail));
                }
            } else {
                ApprovedBookingStatus status = viewModel.status == "abgelaufen"
                    ? ApprovedBookingStatus.EXPIRED
                    : (viewModel.status == "angenommen"
                        ? ApprovedBookingStatus.SCHEDULED
                        : (viewModel.status == "Ladevorgang begonnen"
                            ? ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED
                            : ApprovedBookingStatus.CHARGE_END_CONFIRMED));

                ConnectorType connectorType = ConnectorType.CCS;
                foreach (var a in Enum.GetValues(typeof(ConnectorType)).Cast<ConnectorType>()) {
                    var display = typeof(ConnectorType).GetField(a.ToString())!.GetCustomAttribute<DisplayAttribute>()!.Name;
                    if (display == viewModel.connectorType) {
                        connectorType = a;
                    }
                }

                try {
                    bookingManager.createApprovedBooking(userEmail, viewModel.location, car, (double) viewModel.startSoC / 100,
                        (double) viewModel.targetSoC / 100
                        , viewModel.timePeriods[0], viewModel.chargeGroupName, viewModel.chargeStationName, viewModel.chargePointIndex.Value, status
                        , connectorType);
                }
                catch (CollidingBookingException) {
                    return BadRequest("Die angegebene Buchung kollidiert mit einer bereits bestehenden Buchung");
                }
            }
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminRequired")]
        public IActionResult adminEdit(AdminCreateBookingViewModel viewModel, [FromServices] IUserManager userManager) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }
            Car car;
            var userEmail = viewModel.user.ToLower();
            try {
                car = userManager.getCars(userEmail).First(x => x.name == viewModel.car);
            }
            catch (ArgumentNullException) {
                return BadRequest("Das angegebene Auto wurde im System nicht gefunden");
            }
            catch (InvalidOperationException) {
                return BadRequest("Das Auto existiert im System nicht mehr");
            }
            if (car == null) {
                return BadRequest("Es wurde kein Auto ausgewählt");
            }
            if (viewModel.status == "angefordert" || viewModel.status == "abgelehnt") {
                bool denied = viewModel.status == "abgelehnt";

                try {
                    bookingManager.editRequestedBooking(viewModel.id.Value, denied, userManager.getPriority(userEmail), viewModel.location, car,
                        (double) viewModel.startSoC / 100, (double) viewModel.targetSoC / 100, viewModel.timePeriods, userEmail);
                }
                catch (BookingNotFoundException) {
                    return BadRequest("Diese ID existiert im System nicht");
                }
                catch (WrongBookingTypeException) {
                    bookingManager.deleteBooking(viewModel.id.Value);
                    bookingManager.createRequestedBooking(viewModel.id.Value, userEmail, denied, viewModel.location, car,
                        (double) viewModel.startSoC / 100, (double) viewModel.targetSoC / 100, viewModel.timePeriods,
                        userManager.getPriority(userEmail));
                }
                catch (InvalidOperationException) {
                    return BadRequest("Ein Fehler ist beim Senden der Daten aufgetreten");
                }
            } else {
                ApprovedBookingStatus status = viewModel.status == "abgelaufen"
                    ? ApprovedBookingStatus.EXPIRED
                    : (viewModel.status == "angenommen"
                        ? ApprovedBookingStatus.SCHEDULED
                        : (viewModel.status == "Ladevorgang begonnen"
                            ? ApprovedBookingStatus.CHARGE_BEGIN_CONFIRMED
                            : ApprovedBookingStatus.CHARGE_END_CONFIRMED));

                ConnectorType connectorType;
                try {
                    connectorType = Enum.GetValues(typeof(ConnectorType)).Cast<ConnectorType>().Select(x => new {
                        connectorType = x, displayname = typeof(ConnectorType).GetField(x.ToString())!.GetCustomAttribute<DisplayAttribute>()!.Name
                    }).First(x => x.displayname == viewModel.connectorType).connectorType;
                }
                catch (InvalidOperationException) {
                    if (viewModel.chargeGroupName == null) {
                        return BadRequest("Es wurde noch keine Ladegruppe angegeben");
                    }
                    if (viewModel.chargeStationName == null) {
                        return BadRequest("Es wurde noch keine Ladestation angegeben");
                    }
                    if (viewModel.chargePointIndex == null) {
                        return BadRequest("Es wurde noch keine Ladepunkt angegeben");
                    }
                    if (viewModel.connectorType == null) {
                        return BadRequest("Es wurde noch kein Ladestecker angegeben");
                    }
                    return BadRequest("Es wurden noch nicht alle Daten gesetzt");
                }
                catch (ArgumentException) {
                    return BadRequest("Der angegebene Stecker existiert nicht oder es wurde noch keine Ladestation angegeben");
                }
                try {
                    bookingManager.editApprovedBooking(viewModel.id.Value, status, connectorType, userEmail, viewModel.location, car
                        , viewModel.timePeriods[0], viewModel.chargeGroupName, viewModel.chargeStationName, viewModel.chargePointIndex.Value,
                        (double) viewModel.targetSoC / 100
                        , (double) viewModel.startSoC / 100);
                }
                catch (BookingNotFoundException) {
                    return BadRequest("Diese ID existiert im System nicht");
                }
                catch (WrongBookingTypeException) {
                    try {
                        bookingManager.approveRequestedBookingLock(viewModel.id.Value, viewModel.timePeriods[0], viewModel.chargeGroupName,
                            viewModel.chargeStationName, viewModel.chargePointIndex.Value, connectorType);
                    }
                    catch (CollidingBookingException) {
                        return BadRequest("Die veränderte Buchung kollidiert mit einer bereits bestehenden Buchung");
                    }
                    catch (WrongBookingTypeException) {
                        return BadRequest("Bei Änderungenn der Buchung ist ein Fehler aufgetreten");
                    }
                }
                catch (CollidingBookingException) {
                    return BadRequest("Die veränderte Buchung kollidiert mit einer bereits bestehenden Buchung");
                }
                catch (InvalidOperationException) {
                    return BadRequest("Es wurden noch nicht alle Werte gesetzt");
                }
            }
            return Ok();
        }
    }
}