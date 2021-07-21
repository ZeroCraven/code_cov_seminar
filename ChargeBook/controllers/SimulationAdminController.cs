using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ChargeBook.data.booking;
using chargebook.data.infrastructure;
using chargebook.data.simulation;
using chargebook.data.user;
using chargebook.models;
using ChargeBook.models.booking;
using chargebook.models.infrastructure;
using ChargeBook.models.simulation;
using ChargeBook.models.simulation.settings;
using ChargeBook.models.simulation.simulationLog;
using ChargeBook.services;
using chargebook.viewModels.infrastructure;
using chargebook.viewModels.simulationViewModels;
using ChargeBook.viewModels.simulationViewModels;
using ChargeBook.viewModels.simulationViewModels.infrastructureViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChargeBook.controllers {
    [Authorize(Policy = "AdminRequired")]
    public class SimulationAdminController : Controller {
        private PartialSimulationCache simulationCache;
        private SimulationLogManager simulationLogManager;
        private readonly IInfrastructureManager infrastructureManager;
        private readonly SimulationCarsManager simulationCarsManager;
        private readonly IUserUtils userUtils;

        public SimulationAdminController(PartialSimulationCache cache, SimulationLogManager simLogManager,
            IInfrastructureManager infrastructureManager, SimulationCarsManager simulationCarsManager, IUserUtils userUtils) {
            simulationLogManager = simLogManager;
            simulationCache = cache;
            this.infrastructureManager = infrastructureManager;
            this.simulationCarsManager = simulationCarsManager;
            this.userUtils = userUtils;
        }

        [HttpGet]
        public IActionResult index() {
            ViewData["locations"] = infrastructureManager.getLocationNames();
            return View();
        }

        [HttpGet]
        public IActionResult infrastructure() {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            ViewData["locationName"] = "Simulation";
            ViewData["chargeStationTypes"] = new List<string>(infrastructureManager.possibleChargeStationTypes.Keys);
            return View(sim.infrastructureManager.getInfrastructureByLocation(Simulation.SIMULATION_LOCATION_NAME));
        }

        public IActionResult fleets() {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            return View();
        }

        public IActionResult settings() {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            return View(new SimulationGeneralSettingsViewModel() {
                name = sim.simulationSettings.generalSettings.name,
                seed = sim.simulationSettings.generalSettings.seed,
                tickLength = sim.simulationSettings.generalSettings.tickLength,
                timePeriod = sim.simulationSettings.generalSettings.timePeriod
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult createSimulation([FromServices] IUserManager userManager) {
            string email = userUtils.getEmail(User);
            simulationCache.createSimulation(email, Simulation.SIMULATION_LOCATION_NAME, userManager.possiblePriorityRoles);
            return RedirectToAction("infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> startSimulation(SimulationGeneralSettingsViewModel generalSettings) {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return View("settings", generalSettings);
            }
            sim.setGeneralSettings(generalSettings);

            if (!sim.prepareForStart()) {
                return BadRequest("nicht alle Einstellungen gesetzt");
            }
            await simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User)).runAsync();
            simulationCache.deleteSimulation(userUtils.getEmail(User));
            simulationLogManager.addSimulationLog(sim.id, sim.getLogger());
            return Redirect("/homeadmin/simulationlog?id=" + sim.id);
        }

        [HttpGet]
        public IActionResult getSimulationLog(int id) {
            string json = JsonConvert.SerializeObject(simulationLogManager.getSimulationLogById(id));
            UTF8Encoding encoding = new UTF8Encoding();
            Response.Headers["Content-Length"] = encoding.GetBytes(json).Length.ToString();
            return Ok(json);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult setBookingGenerationSettings(List<BookingGenerationSettingViewModel> viewModels) {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return Ok(JsonConvert.SerializeObject(new {
                    success = false, redirect = "index",
                }));
            }
            List<BookingGenerationSetting> bookingGenerationSettings = new List<BookingGenerationSetting>();
            // return early without error because no fleet specified
            if (viewModels == null || !viewModels.Any()) {
                sim.setBookingGenerationSettings(bookingGenerationSettings);
                return Ok(JsonConvert.SerializeObject(new {
                    success = true
                }));
            }
            if (!ModelState.IsValid) {
                return Ok(JsonConvert.SerializeObject(new {
                    success = false, message = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage
                }));
            }
            foreach (var viewModel in viewModels) {
                Car car = simulationCarsManager.getCars().Find(x => x.name == viewModel.carName);
                List<Tuple<TimeSpan, TimeSpan>> requestedTimeSpans = viewModel.requestedSimulationTimePeriods
                    .Select(requestedSimulationTimePeriod => new Tuple<TimeSpan, TimeSpan>(
                        TimeSpan.FromMinutes(requestedSimulationTimePeriod.minutesFromMidnightStart),
                        TimeSpan.FromMinutes(requestedSimulationTimePeriod.minutesFromMidnightEnd))).ToList();

                bookingGenerationSettings.Add(new BookingGenerationSetting() {
                    bookingDays = viewModel.bookingDays,
                    car = car,
                    chargedEnergyInPercent = (double) viewModel.chargedEnergyInPercent / 100,
                    count = viewModel.count,
                    priorityRole = viewModel.priorityRole,
                    requestedTimeSpans = requestedTimeSpans,
                    requestFrequency = viewModel.perWeekRequestFrequency,
                    timeOfRequest = TimeSpan.FromHours(viewModel.requestTime),
                });
            }
            sim.setBookingGenerationSettings(bookingGenerationSettings);
            return Ok(JsonConvert.SerializeObject(new {
                success = true
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult createCar(Car car) {
            if (!ModelState.IsValid) {
                return Ok(JsonConvert.SerializeObject(new {
                    success = false,
                    message = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage,
                    cars = simulationCarsManager.getCars(),
                }));
            }
            try {
                simulationCarsManager.addCar(car);
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
        public IActionResult cars() {
            var cars = simulationCarsManager.getCars();
            return Ok(JsonConvert.SerializeObject(new {
                cars = cars
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult deleteCar(string carName) {
            if (string.IsNullOrEmpty(carName) || !simulationCarsManager.existsCar(carName)) {
                return BadRequest();
            }

            simulationCarsManager.deleteCar(carName);
            return Ok(JsonConvert.SerializeObject(new {
                success = true, message = $"Das Fahrzeug \"{carName}\" wurde erfolgreich gelöscht"
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Obsolete]
        public IActionResult resetSimulationInfrastructure() {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                return RedirectToAction("index");
            }
            Infrastructure simulationInfrastructure = sim.infrastructureManager.getInfrastructureByLocation(Simulation.SIMULATION_LOCATION_NAME);
            simulationInfrastructure.chargeGroups = new Dictionary<string, ChargeGroup>();
            return RedirectToAction("createSimulation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult createChargeGroup(SimulationCreateChargeGroupViewModel viewModel) {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("infrastructure");
            }
            try {
                sim.infrastructureManager.createChargeGroup(Simulation.SIMULATION_LOCATION_NAME, viewModel.chargeGroupName, viewModel.maxChargePower);
            }
            catch (InfrastructurePartAlreadyExistsException e) {
                TempData["errorMessage"] = e.Message;
            }
            return RedirectToAction("Infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult deleteChargeGroup(SimulationDeleteChargeGroupViewModel viewModel) {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("infrastructure");
            }
            try {
                sim.infrastructureManager.deleteChargeGroup(Simulation.SIMULATION_LOCATION_NAME, viewModel.chargeGroupName);
            }
            catch (InfrastructurePartNotFoundException e) {
                TempData["errorMessage"] = e.Message;
            }
            return RedirectToAction("infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult createChargeStation(SimulationCreateChargeStationViewModel viewModel) {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("infrastructure");
            }
            try {
                sim.infrastructureManager.createChargeStation(Simulation.SIMULATION_LOCATION_NAME,
                    viewModel.chargeGroupName, viewModel.chargeStationName, viewModel.chargeStationTypeName);
            }
            catch (InfrastructurePartAlreadyExistsException e) {
                TempData["errorMessage"] = e.Message;
            }
            catch (InfrastructurePartNotFoundException e) {
                TempData["errorMessage"] = e.Message;
            }
            return RedirectToAction("infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult deleteChargeStation(SimulationDeleteChargeStationViewModel viewModel) {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("infrastructure");
            }
            try {
                sim.infrastructureManager.deleteChargeStation(Simulation.SIMULATION_LOCATION_NAME,
                    viewModel.chargeGroupName, viewModel.chargeStationName);
            }
            catch (InfrastructurePartAlreadyExistsException e) {
                TempData["errorMessage"] = e.Message;
            }
            catch (InfrastructurePartNotFoundException e) {
                TempData["errorMessage"] = e.Message;
            }
            return RedirectToAction("infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult setInfrastructureSettings(SimulationInfrastructureSettingsViewModel viewModel) {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("infrastructure");
            }
            sim.infrastructureManager.setInfrastructureSettings(Simulation.SIMULATION_LOCATION_NAME, viewModel.beginBuffer, viewModel.endBuffer,
                viewModel.minReservedCCSConnectors,
                viewModel.minReservedChademo, viewModel.minReservedType2);
            return RedirectToAction("infrastructure");
        }

        [HttpGet]
        public IActionResult exportScenario(int simulationId) {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] contentAsBytes = encoding.GetBytes(JsonConvert.SerializeObject(simulationLogManager.getSimulationLogById(simulationId).scenario));
            return File(contentAsBytes, "application/json", "scenario.json");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult importInfrastructureFromLocation(LocationViewModel viewModel, [FromServices] IUserManager userManager) {
            if (!ModelState.IsValid) {
                TempData["errorMessage"] = ModelState.First(pair => pair.Value.Errors.Count > 0).Value.Errors.First().ErrorMessage;
                return RedirectToAction("index");
            }
            string email = userUtils.getEmail(User);
            simulationCache.createSimulation(email, Simulation.SIMULATION_LOCATION_NAME, userManager.possiblePriorityRoles);
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            Infrastructure importedInfrastructure = infrastructureManager.getInfrastructureByLocation(viewModel.location).deepCopy();
            sim.infrastructureManager.getInfrastructureByLocation(Simulation.SIMULATION_LOCATION_NAME).chargeGroups =
                importedInfrastructure.chargeGroups;
            sim.infrastructureManager.getInfrastructureByLocation(Simulation.SIMULATION_LOCATION_NAME).infrastructureSettings =
                importedInfrastructure.infrastructureSettings;
            return RedirectToAction("infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Obsolete]
        public IActionResult importInfrastructureAndBookingsFromLocation([FromServices] IBookingManager bookingManager, LocationViewModel viewModel,
            [FromServices] IUserManager userManager) {
            if (!ModelState.IsValid) {
                return BadRequest();
            }
            string email = userUtils.getEmail(User);
            simulationCache.createSimulation(email, Simulation.SIMULATION_LOCATION_NAME, userManager.possiblePriorityRoles);

            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            Infrastructure importedInfrastructure = infrastructureManager.getInfrastructureByLocation(viewModel.location).deepCopy();
            sim.infrastructureManager.getInfrastructureByLocation(Simulation.SIMULATION_LOCATION_NAME).chargeGroups =
                importedInfrastructure.chargeGroups;
            sim.infrastructureManager.getInfrastructureByLocation(Simulation.SIMULATION_LOCATION_NAME).infrastructureSettings =
                importedInfrastructure.infrastructureSettings;

            sim.bookingsFromLocation = bookingManager.getBookingsByLocation(viewModel.location).ToList();
            return RedirectToAction("infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Obsolete]
        public IActionResult importInfrastructureFromScenario(int id, [FromServices] IUserManager userManager) {
            Infrastructure importedInfrastructure;
            try {
                importedInfrastructure = simulationLogManager.getScenarioBySimulationLogId(id).simulatedInfrastructure;
            }
            catch (ArgumentException) {
                return BadRequest();
            }
            simulationCache.importInfrastructure(userUtils.getEmail(User), Simulation.SIMULATION_LOCATION_NAME, importedInfrastructure.deepCopy(),
                userManager.possiblePriorityRoles);
            return RedirectToAction("infrastructure");
        }

        [HttpGet]
        public IActionResult historyApi() {
            return Ok(JsonConvert.SerializeObject(simulationLogManager.getHistory()));
        }

        [HttpGet]
        public IActionResult bookingGenerationSettings() {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                return BadRequest();
            }
            List<BookingGenerationSettingViewModel> list = new List<BookingGenerationSettingViewModel>();
            foreach (var bookingGenerationSetting in sim.simulationSettings.bookingGenerationSettings) {
                var requestedTimeSpans = new List<RequestedSimulationTimePeriodViewModel>();
                foreach (var timeSpan in bookingGenerationSetting.requestedTimeSpans) {
                    requestedTimeSpans.Add(new RequestedSimulationTimePeriodViewModel() {
                        minutesFromMidnightStart = timeSpan.Item1.Minutes + timeSpan.Item1.Hours * 60,
                        minutesFromMidnightEnd = timeSpan.Item2.Minutes + timeSpan.Item2.Hours * 60
                    });
                }
                list.Add(new BookingGenerationSettingViewModel() {
                    carName = bookingGenerationSetting.car.name,
                    bookingDays = bookingGenerationSetting.bookingDays,
                    chargedEnergyInPercent = (int) (bookingGenerationSetting.chargedEnergyInPercent * 100),
                    count = bookingGenerationSetting.count,
                    perWeekRequestFrequency = bookingGenerationSetting.requestFrequency,
                    priorityRole = bookingGenerationSetting.priorityRole,
                    requestedSimulationTimePeriods = requestedTimeSpans,
                    requestTime = (int) (bookingGenerationSetting.timeOfRequest.TotalHours)
                });
            }
            return Ok(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult importScenario(int id, [FromServices] IUserManager userManager) {
            string email = userUtils.getEmail(User);
            simulationCache.createSimulation(email, Simulation.SIMULATION_LOCATION_NAME, userManager.possiblePriorityRoles);
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                return RedirectToAction("index");
            }
            Infrastructure importedInfrastructure;
            try {
                importedInfrastructure = simulationLogManager.getScenarioBySimulationLogId(id).simulatedInfrastructure.deepCopy();
                sim.simulationSettings = simulationLogManager.getScenarioBySimulationLogId(id).simulationSettings.deepCopy();
            }
            catch (ArgumentException) {
                return BadRequest();
            }
            simulationCache.importInfrastructure(userUtils.getEmail(User), Simulation.SIMULATION_LOCATION_NAME, importedInfrastructure,
                userManager.possiblePriorityRoles);
            return RedirectToAction("infrastructure");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult importScenarioFromFile(IFormFile file, [FromServices] IUserManager userManager) {
            if (file == null || !file.FileName.EndsWith(".json")) {
                TempData["errorMessage"] = "Ungültige Datei";
                return RedirectToAction("index");
            }
            if (file.Length > 10000000) {
                TempData["errorMessage"] = "Die Datei darf nicht größer als 10mb sein";
                return RedirectToAction("index");
            }
            string email = userUtils.getEmail(User);
            simulationCache.createSimulation(email, Simulation.SIMULATION_LOCATION_NAME, userManager.possiblePriorityRoles);
            Scenario scenario;
            try {
                scenario = JsonConvert.DeserializeObject<Scenario>(new StreamReader(file.OpenReadStream()).ReadToEnd());
                if (scenario.simulatedInfrastructure == null || scenario.simulationSettings == null) {
                    throw new JsonException("Could not parse Scenario");
                }
            }
            catch (JsonException) {
                TempData["errorMessage"] = "Ungültige Datei";
                return RedirectToAction("index");
            }
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            simulationCache.importInfrastructure(userUtils.getEmail(User), Simulation.SIMULATION_LOCATION_NAME, scenario.simulatedInfrastructure,
                userManager.possiblePriorityRoles);
            sim.simulationSettings = scenario.simulationSettings;
            return RedirectToAction("infrastructure", "SimulationAdmin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Obsolete]
        public IActionResult importSettingsFromScenario(int id) {
            Simulation sim = simulationCache.getSimulationByCreatorEmail(userUtils.getEmail(User));
            if (sim == null) {
                TempData["errorMessage"] = "Sie wurden zurückgeleitet, weil keine Simulation von Ihnen in Bearbeitung war";
                return RedirectToAction("index");
            }
            try {
                sim.simulationSettings = simulationLogManager.getScenarioBySimulationLogId(id).simulationSettings.deepCopy();
            }
            catch (ArgumentException) {
                return BadRequest();
            }
            return RedirectToAction("infrastructure");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult deleteSimulationLog(int id) {
            try {
                simulationLogManager.deleteSimulationLog(id);
            }
            catch (ArgumentException) {
                return Ok(JsonConvert.SerializeObject(new {
                    success = false, message = "Das Löschen des Simulationsprotokolls ist fehlgeschlagen"
                }));
            }
            return Ok(JsonConvert.SerializeObject(new {
                success = true
            }));
        }
    }
}