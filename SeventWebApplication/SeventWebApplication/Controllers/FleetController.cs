using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Booking;
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
        private readonly ImageUploadService _images;
        private readonly VehicleAssignmentService _assignments;
        private readonly BookingRepository _bookings;
        private readonly IUserRepository _users;

        public FleetController(UserSession session, FleetManager fleet, VehicleDamageService damages, ImageUploadService images,
            VehicleAssignmentService assignments, BookingRepository bookings, IUserRepository users)
            : base(session)
        {
            _fleet = fleet;
            _damages = damages;
            _images = images;
            _assignments = assignments;
            _bookings = bookings;
            _users = users;
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
        public async Task<IActionResult> Create(VehicleForm form)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            if (!ModelState.IsValid)
                return View(form);

            try
            {
                // Kolla bilden först, så fordonet inte läggs till om den är ogiltig
                if (form.Image is { Length: > 0 })
                    _images.Validate(form.Image);

                Vehicle vehicle = BuildVehicle(form);
                _fleet.AddVehicle(vehicle);

                if (form.Image is { Length: > 0 })
                    vehicle.ImagePath = await _images.SaveAsync(form.Image, "vehicles");

                // Bilen kopplas till en familj av VehicleAssignmentService (via VehicleAdded), så vi kan visa vilken
                string name = $"{vehicle.Model.Brand.BrandName} {vehicle.Model.ModelName}";
                VehicleFamily? family = _assignments.GetFamilyOf(vehicle.VehicleId);

                if (family != null)
                {
                    TempData["Success"] = $"{name} lades till i familjen {FamilyLabels.For(family)}";
                    TempData["SuccessLink"] = Url.Action("Details", "Families", new { id = family.FamilyId });
                    TempData["SuccessLinkText"] = "Visa familjen";
                }
                else
                {
                    TempData["Success"] = $"{name} ({vehicle.RegistrationPlate.RegNr}) lades till i flottan, men matchar ingen familj ännu.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or NotSupportedException)
            {
                // Valideringen i factories kastar med ett läsbart meddelande, det visar vi i formuläret
                ModelState.AddModelError("", ex.Message);
                return View(form);
            }
        }

        // ---- Detaljvy ----

        // GET /Fleet/Details/{id} – regnummer, familj, status och bokningshistorik
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            Vehicle? v = _fleet.GetVehicle(id);
            if (v is null)
                return NotFound();

            VehicleFamily? family = _assignments.GetFamilyOf(id);
            DateTime now = DateTime.Now;

            return View(new VehicleDetailsViewModel
            {
                VehicleId = v.VehicleId,
                RegNr = v.RegistrationPlate.RegNr,
                Name = $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                Type = v is Van ? "Van" : "Car",
                Year = v.Year,
                ImagePath = v.ImagePath,
                Status = v.VehicleStatus,
                IsRented = _damages.IsCurrentlyRented(id),
                Transmission = v.TransmissionType.ToString(),
                FuelType = v.Engine.FuelType.ToString(),
                PassengerCapacity = v.PassengerCapacity,
                LuggageCapacity = v.LuggageCapacity,
                FamilyId = family?.FamilyId,
                FamilyLabel = family is null ? null : FamilyLabels.For(family),
                Bookings = _bookings.GetByVehicleId(id)
                    .OrderByDescending(b => b.StartDate)
                    .Select(b => BookingRowViewModel.From(b, _users, now))
                    .ToList(),
                Damages = _damages.GetDamagesByVehicle(id)
                    .OrderByDescending(d => d.ReportedAt)
                    .Select(d => new DamageRowViewModel
                    {
                        DamageId = d.DamageId,
                        RegNr = v.RegistrationPlate.RegNr,
                        Vehicle = $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                        Description = d.Description,
                        ReportedAt = d.ReportedAt,
                        IsResolved = d.IsResolved
                    }).ToList()
            });
        }

        // ---- Redigera / radera ----

        // GET /Fleet/Edit/{id}
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            Vehicle? v = _fleet.GetVehicle(id);
            if (v is null)
                return NotFound();

            return View(new VehicleEditForm
            {
                VehicleId = id,
                TypeName = v is Van ? "Van" : "Car",
                RegNr = v.RegistrationPlate.RegNr,
                CountryCode = v.RegistrationPlate.CountryCode,
                Brand = v.Model.Brand.BrandName,
                ModelName = v.Model.ModelName,
                Year = v.Year,
                PassengerCapacity = v.PassengerCapacity,
                LuggageCapacity = v.LuggageCapacity,
                CurrentImagePath = v.ImagePath
            });
        }

        // POST /Fleet/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VehicleEditForm form)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            Vehicle? v = _fleet.GetVehicle(form.VehicleId);
            if (v is null)
                return NotFound();

            form.CurrentImagePath = v.ImagePath;
            form.TypeName = v is Van ? "Van" : "Car";

            if (!ModelState.IsValid)
                return View(form);

            try
            {
                if (form.Image is { Length: > 0 })
                    _images.Validate(form.Image);

                RegNrValidatorFactory.Create(form.CountryCode).ValidateRegNr(form.RegNr);
                var brandModelValidator = new BrandModelValidator();
                brandModelValidator.ValidateBrand(form.Brand);
                brandModelValidator.ValidateModel(form.ModelName);
                new YearValidator().ValidateYear(form.Year);

                if (form.PassengerCapacity < 1)
                    throw new ArgumentException("Passenger capacity must be at least 1.");
                if (form.LuggageCapacity < 0)
                    throw new ArgumentException("Luggage capacity cannot be negative.");

                VehicleType type = v is Van ? VehicleType.Van : VehicleType.Car;
                var brand = new VehicleBrand(form.Brand.Trim(), type);
                var model = new VehicleModel(form.ModelName.Trim(), brand.BrandId, type) { Brand = brand };
                var plate = new RegistrationPlate(form.RegNr.Trim().ToUpperInvariant(), form.CountryCode);

                _fleet.UpdateVehicle(v.VehicleId, plate, model, form.Year, form.PassengerCapacity, form.LuggageCapacity);

                if (form.Image is { Length: > 0 })
                    v.ImagePath = await _images.SaveAsync(form.Image, "vehicles");

                // Skåpbilsfamiljer matchar bl.a. på antal platser, så räkna om kopplingen
                _assignments.ReassignFamily(v);

                TempData["Success"] = $"{model.Brand.BrandName} {model.ModelName} ({plate.RegNr}) was updated.";
                return RedirectToAction(nameof(Details), new { id = v.VehicleId });
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or NotSupportedException)
            {
                ModelState.AddModelError("", ex.Message);
                return View(form);
            }
        }

        // POST /Fleet/Delete – blockeras om bilen är uthyrd eller har öppna bokningar
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            return RunAndRedirect(_ => _fleet.RemoveVehicle(id), "Vehicle deleted.", nameof(Index));
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
                    RegNr = f.RegNr.Trim().ToUpperInvariant(),
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
                RegNr = f.RegNr.Trim().ToUpperInvariant(),
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
                    ImagePath = v.ImagePath,
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
