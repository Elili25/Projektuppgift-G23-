using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Admin: fordonsflottan (/Fleet). Lista alla fordon, lägga till bil eller skåpbil (UC2)
    // och sätta fordon under service (UC3). Reglerna ligger i Vehicle/FleetManager/factories,
    // här tar vi bara emot formulär och visar resultatet.
    public class FleetController : StaffController
    {
        private readonly FleetManager _fleet;
        private readonly VehicleDamageService _damages;

        public FleetController(UserSession session, FleetManager fleet, VehicleDamageService damages)
            : base(session)
        {
            _fleet = fleet;
            _damages = damages;
        }

        // GET /Fleet – alla fordon med status
        [HttpGet]
        public IActionResult Index()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(BuildRows(_fleet.GetAllVehicles()));
        }

        // GET /Fleet/Service – bara fordon som är InService eller InRepair
        [HttpGet]
        public IActionResult Service()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            var outOfOrder = _fleet.GetAllVehicles()
                .Where(v => v.VehicleStatus is VehicleStatus.InService or VehicleStatus.InRepair);

            return View(BuildRows(outOfOrder));
        }

        // POST /Fleet/MarkInService – Active → InService (UC3)
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult MarkInService(Guid id)
        {
            return RunAndRedirect(_ =>
            {
                Vehicle vehicle = _fleet.GetVehicle(id) ?? throw new InvalidOperationException("Vehicle not found.");

                if (vehicle.VehicleStatus != VehicleStatus.Active)
                    throw new InvalidOperationException($"Vehicle is {vehicle.VehicleStatus}, only active vehicles can be put in service.");

                if (_damages.IsCurrentlyRented(id) || !vehicle.IsAvailable)
                    throw new InvalidOperationException("Vehicle is rented out right now and cannot be put in service.");

                vehicle.ChangeStatus(VehicleStatus.InService);
            }, "Vehicle marked as in service.", nameof(Index));
        }

        // POST /Fleet/EndService – InService → Active (eller InRepair om det finns öppna skador)
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EndService(Guid id)
        {
            return RunAndRedirect(_ =>
            {
                Vehicle vehicle = _fleet.GetVehicle(id) ?? throw new InvalidOperationException("Vehicle not found.");

                if (vehicle.VehicleStatus != VehicleStatus.InService)
                    throw new InvalidOperationException("Only vehicles in service can be returned to service.");

                if (_damages.HasOpenDamages(id))
                    throw new InvalidOperationException("Vehicle has unresolved damages. Resolve them under Returns & inspections → Damages.");

                vehicle.ChangeStatus(VehicleStatus.Active);
            }, "Vehicle is active again.", nameof(Service));
        }

        // ---- Add vehicle (UC2) ----

        [HttpGet]
        public IActionResult Create(VehicleType? type)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(new VehicleForm { Type = type ?? VehicleType.Car });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(VehicleForm form)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            if (!ModelState.IsValid)
                return View(form);

            try
            {
                Vehicle vehicle = BuildVehicle(form);
                _fleet.AddVehicle(vehicle);

                TempData["Success"] = $"{vehicle.Model.Brand.BrandName} {vehicle.Model.ModelName} ({vehicle.RegistrationPlate.RegNr}) was added to the fleet.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or NotSupportedException)
            {
                // Valideringen i factories kastar med ett läsbart meddelande, det visar vi i formuläret
                ModelState.AddModelError("", ex.Message);
                return View(form);
            }
        }

        // Bygger rätt fordon via CarFactory/VanFactory beroende på vald typ.
        // Märke och modell skapas från texten i formuläret (det finns inget register i appen än).
        private static Vehicle BuildVehicle(VehicleForm f)
        {
            var regNrValidator = RegNrValidatorFactory.Create(f.CountryCode);
            var brandModelValidator = new BrandModelValidator();
            var yearValidator = new YearValidator();
            var hpValidator = new HorsePowerValidator();

            brandModelValidator.ValidateBrand(f.Brand);
            brandModelValidator.ValidateModel(f.ModelName);

            var brand = new VehicleBrand(f.Brand.Trim(), f.Type);
            var model = new VehicleModel(f.ModelName.Trim(), brand.BrandId, f.Type) { Brand = brand };

            if (f.Type == VehicleType.Van)
            {
                var factory = new VanFactory(regNrValidator, brandModelValidator, yearValidator, hpValidator);
                return factory.Create(new VanCreateRequest
                {
                    RegNr = f.RegNr.Trim(),
                    CountryCode = f.CountryCode,
                    Model = model,
                    Year = f.Year,
                    HorsePower = f.HorsePower,
                    PassengerCapacity = f.PassengerCapacity,
                    LuggageCapacity = f.LuggageCapacity,
                    FuelType = f.FuelType,
                    TransmissionType = f.TransmissionType,
                    FuelCapacityLiters = f.FuelCapacityLiters,
                    BatteryCapacityKwh = f.BatteryCapacityKwh,
                    RangeKm = f.RangeKm,
                    CategoryType = f.VanCategory,
                    CargoCapacityM3 = f.CargoCapacityM3,
                    MaxPayloadKg = f.MaxPayloadKg
                });
            }

            var carFactory = new CarFactory(regNrValidator, brandModelValidator, yearValidator, hpValidator);
            return carFactory.Create(new CarCreateRequest
            {
                RegNr = f.RegNr.Trim(),
                CountryCode = f.CountryCode,
                Model = model,
                Year = f.Year,
                HorsePower = f.HorsePower,
                PassengerCapacity = f.PassengerCapacity,
                LuggageCapacity = f.LuggageCapacity,
                FuelType = f.FuelType,
                TransmissionType = f.TransmissionType,
                FuelCapacityLiters = f.FuelCapacityLiters,
                BatteryCapacityKwh = f.BatteryCapacityKwh,
                RangeKm = f.RangeKm,
                CategoryType = f.CarCategory,
                ComfortType = f.Comfort
            });
        }

        private List<VehicleRowViewModel> BuildRows(IEnumerable<Vehicle> vehicles)
        {
            return vehicles
                .OrderBy(v => v.RegistrationPlate.RegNr)
                .Select(v => new VehicleRowViewModel
                {
                    VehicleId = v.VehicleId,
                    RegNr = v.RegistrationPlate.RegNr,
                    Name = $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                    Type = v is Van ? "Van" : "Car",
                    Year = v.Year,
                    Status = v.VehicleStatus,
                    IsRented = _damages.IsCurrentlyRented(v.VehicleId),
                    OpenDamages = _damages.GetDamagesByVehicle(v.VehicleId).Count(d => !d.IsResolved)
                })
                .ToList();
        }
    }
}
