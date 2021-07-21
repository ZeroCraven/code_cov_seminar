using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using ChargeBook.data.booking;
using chargebook.data.user;
using chargebook.models;
using ChargeBook.services;
using chargebook.viewModels;
using chargebook.viewModels.infrastructure;
using ChargeBook.viewModels.userViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ChargeBook.controllers {
    public class UserController : Controller {
        private readonly IUserManager userManager;
        private readonly IUserUtils userUtils;

        public UserController(IUserManager userManager, IUserUtils userUtils) {
            this.userManager = userManager;
            this.userUtils = userUtils;
        }

        [HttpGet]
        public IActionResult login(string returnUrl) {
            if (userUtils.isLoggedIn(User)) {
                return RedirectToAction("index", "Home");
            }
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> login(string email, string password, string keepLoggedIn, string returnUrl = null) {
            returnUrl ??= Url.Content("~/");
            var identity = userManager.findIdentity(email.ToLower(), password);
            if (identity != null) {
                await HttpContext.SignInAsync(new ClaimsPrincipal(identity), new AuthenticationProperties() {
                    IsPersistent = keepLoggedIn == "on"
                });
                return LocalRedirect(returnUrl);
            }
            TempData["logInFailed"] = true;
            ViewData["email"] = email;
            return View();
        }

        public async Task<IActionResult> logout() {
            await HttpContext.SignOutAsync();
            return RedirectToAction("login");
        }

        [HttpPost]
        [Authorize(Policy = "LoggedInRequired")]
        [ValidateAntiForgeryToken]
        public IActionResult selectOnBehalf(string onBehalf) {
            try {
                userManager.changeSelectedOnBehalf(userUtils.getEmail(User), onBehalf);
            }
            catch (UserNotAuthorisedException) { return BadRequest(); }
            catch (UserNotFoundException) { return BadRequest(); }
            return RedirectToAction("index", "home");
        }

        [HttpPost]
        [Authorize(Policy = "LoggedInRequired")]
        [ValidateAntiForgeryToken]
        public IActionResult changePassword(string oldPassword, string newPassword) {
            if (userManager.changePassword(userUtils.getEmail(User), oldPassword, newPassword)) {
                return Ok();
            } else {
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "LoggedInRequired")]
        public IActionResult createCar(Car car) {
            if (!ModelState.IsValid) {
                return Ok(JsonConvert.SerializeObject(new {
                    success = false,
                    message = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage,
                    cars = userUtils.getCars(User)
                }));
            }
            try {
                userManager.addCar(userUtils.getEmail(User), car);
            }
            catch (ArgumentException) {
                return Ok(JsonConvert.SerializeObject(new {
                    success = false, message = $"Das Fahrzeug \"{car.name}\" existiert bereits"
                }));
            }
            return Ok(JsonConvert.SerializeObject(new {
                success = true, message = $"Das Fahrzeug \"{car.name}\" wurde erfolgreich angelegt"
            }));
        }

        [HttpGet]
        [Authorize(Policy = "LoggedInRequired")]
        public IActionResult cars(string userEmail = null) {
            if (string.IsNullOrEmpty(userEmail) || userEmail == userUtils.getEmail(User)) {
                var cars = userUtils.getCars(User);
                return Ok(JsonConvert.SerializeObject(new {
                    cars = cars
                }));
            }
            if (!userUtils.isAdmin(User)) {
                return Forbid();
            }
            try {
                var cars = userManager.getCars(userEmail);
                return Ok(JsonConvert.SerializeObject(new {
                    cars = cars
                }));
            }
            catch (UserNotFoundException) {
                return BadRequest("User not Found");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "LoggedInRequired")]
        public IActionResult deleteCar(string carName) {
            if (string.IsNullOrEmpty(carName)) {
                return BadRequest();
            }
            userManager.deleteCar(userUtils.getEmail(User), carName);
            return Ok(JsonConvert.SerializeObject(new {
                success = true, message = $"Das Fahrzeug \"{carName}\" wurde erfolgreich gelöscht"
            }));
        }

        [HttpPost]
        [Authorize(Policy = "AdminRequired")]
        [ValidateAntiForgeryToken]
        public IActionResult edit(EditUserViewModel editUserViewModel) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }
            try {
                userManager.editUser(editUserViewModel.email, editUserViewModel.firstName, editUserViewModel.lastName, editUserViewModel.priorityRole,
                    editUserViewModel.isAdmin);
            }
            catch (UserNotFoundException) {
                return BadRequest("Es existiert kein verifizierter Nutzer mit dieser E-mail");
            }
            return Ok();
        }

        [HttpPost]
        [Authorize(Policy = "AdminRequired")]
        [ValidateAntiForgeryToken]
        public IActionResult deleteUnverifiedUser([Required] string email) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }
            try {
                userManager.deleteUnverifiedUser(email);
            }
            catch (UserNotFoundException) {
                return BadRequest("Es existiert kein unverifizierter Nutzer mit dieser E-mail");
            }
            return Ok();
        }

        [HttpPost]
        [Authorize(Policy = "AdminRequired")]
        [ValidateAntiForgeryToken]
        public IActionResult deleteVerifiedUser([Required] [EmailAddress(ErrorMessage = "Diesen Nutzer kann man nicht löschen"),]
            string email,
            [FromServices] IBookingManager bookingManager) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }
            try {
                userManager.deleteVerifiedUser(email);
                bookingManager.deleteBookingsFromUser(email);
            }
            catch (UserNotFoundException) {
                return BadRequest("Es existiert kein verifizierter Nutzer mit dieser E-mail");
            }
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminRequired")]
        public async Task<IActionResult> create(CreateUserViewModel createUserViewModel) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }
            using var scope = HttpContext.RequestServices.CreateScope();
            try {
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var token = userManager.createUnverifiedUser(createUserViewModel.email.ToLower(), createUserViewModel.isAdmin,
                    createUserViewModel.priorityRole);
                var scheme = HttpContext.Request.Scheme;
                var registrationLink = Url.Action("register", "User", new {
                    email = createUserViewModel.email, verificationToken = token
                }, scheme);
                var renderService = scope.ServiceProvider.GetRequiredService<IViewRenderService>();
                var emailString = await renderService.renderToStringAsync("email/registerEmail", registrationLink);
                await emailSender.sendEmailAsync(createUserViewModel.email, "Registrierung für Chargebook", emailString,
                    emailString, "MHP Mitarbeiter");
            }
            catch (UserAlreadyExistsException) {
                return BadRequest("Mit dieser Email existiert schon ein User");
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult register([Required] string email, [Required] string verificationToken) {
            if (email == null || verificationToken == null || !userManager.checkUnverifiedUserVerificationToken(email, verificationToken)) {
                return View("tokenError");
            }
            return View(new RegisterUserViewModel() {
                email = email, verificationToken = verificationToken
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> register(RegisterUserViewModel registerUserViewModel) {
            if (!ModelState.IsValid) {
                return View(registerUserViewModel);
            }
            try {
                userManager.registerUnverifiedUser(registerUserViewModel.email.ToLower(), registerUserViewModel.verificationToken,
                    registerUserViewModel.password, registerUserViewModel.firstName, registerUserViewModel.lastName,
                    registerUserViewModel.defaultLocation);
            }
            catch (ValidTokenEmailCombinationNotFoundException) {
                return View("tokenError");
            }

            TempData["registerSuccessful"] = true;
            await HttpContext.SignOutAsync();
            return RedirectToAction("login");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> forgotPassword([Required] string email) {
            if (!ModelState.IsValid) {
                return BadRequest("Email darf nicht leer sein");
            }
            using var scope = HttpContext.RequestServices.CreateScope();
            try {
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var token = userManager.prepareForPasswordReset(email.ToLower());
                var scheme = HttpContext.Request.Scheme;
                var resetPasswordLink = Url.Action("resetPassword", "User", new {
                    email = email, resetPasswordToken = token
                }, scheme);
                var renderService = scope.ServiceProvider.GetRequiredService<IViewRenderService>();
                var emailString = await renderService.renderToStringAsync("email/resetPasswordEmail", resetPasswordLink);
                await emailSender.sendEmailAsync(email, "Passwort von Chargebook zurücksetzen", emailString,
                    emailString, "MHP Mitarbeiter");
            }
            catch (UserNotFoundException) {
                return BadRequest("Die Email existiert im System nicht");
            }
            return Ok();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> resetPassword(ResetPasswordViewModel resetPasswordViewModel) {
            if (!ModelState.IsValid) {
                return View(resetPasswordViewModel);
            }
            try {
                userManager.resetPassword(resetPasswordViewModel.email.ToLower(), resetPasswordViewModel.resetPasswordToken,
                    resetPasswordViewModel.password);
            }
            catch (ValidTokenEmailCombinationNotFoundException) {
                return View("tokenError");
            }
            TempData["passwordChangeSuccessful"] = true;
            if (userUtils.isLoggedIn(User)) {
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("login");
        }

        [HttpGet]
        public IActionResult resetPassword([Required] string email, [Required] string resetPasswordToken) {
            if (!ModelState.IsValid) {
                return BadRequest();
            }
            if (!userManager.checkResetPasswordToken(email.ToLower(), resetPasswordToken)) {
                return View("tokenError");
            }
            return View(new ResetPasswordViewModel() {
                email = email, resetPasswordToken = resetPasswordToken
            });
        }

        [HttpGet]
        public IActionResult forbidden() {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "AdminRequired")]
        public IActionResult users() {
            return Ok(JsonConvert.SerializeObject(new {
                verifiedUsers = userManager.getVerifiedUsersAsViewModel(), unverifiedUsers = userManager.getUnverifiedUsersAsViewModel()
            }));
        }

        [HttpPost]
        [Authorize(Policy = "LoggedInRequired")]
        [ValidateAntiForgeryToken]
        public IActionResult setDefaultLocation(LocationViewModel viewModel) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }
            userManager.setDefaultLocation(userUtils.getEmail(User), viewModel.location);
            return Ok();
        }

        [HttpPost]
        [Authorize(Policy = "LoggedInRequired")]
        [ValidateAntiForgeryToken]
        public IActionResult setNotification(bool enabled) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage);
            }
            userManager.setNotificationEnabled(userUtils.getEmail(User), enabled);
            return Ok();
        }
    }
}