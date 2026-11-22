using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Gör fordonsfamiljerna nåbara från webben: lista familjer och kolla om en familj är ledig.
    // Manage/Create är admin-tabben där personal kan se och skapa familjer.
    public class FamiliesController : Controller
    {
        private readonly VehicleFamilyRepository _familyRepository;
        private readonly VehicleAssignmentService _assignmentService;
        private readonly VehicleFamilyService _familyService;
        private readonly UserSession _session;
        private readonly ImageUploadService _images;
        private readonly BookingRepository _bookings;
        private readonly VehicleDamageService _damages;

        public FamiliesController(
            VehicleFamilyRepository familyRepository,
            VehicleAssignmentService assignmentService,
            VehicleFamilyService familyService,
            UserSession session,
            ImageUploadService images,
            BookingRepository bookings,
            VehicleDamageService damages)
        {
            _bookings = bookings;
            _damages = damages;
            _familyRepository = familyRepository;
            _assignmentService = assignmentService;
            _familyService = familyService;
            _session = session;
            _images = images;
        }

        // GET /Families  – alla familjer med antal matchande bilar
        [HttpGet]
        public IActionResult Index()
        {
            var result = _familyRepository.GetAll().Select(f => new
            {
                f.FamilyId,
                f.Name,
                f.Description,
                VehicleCount = _assignmentService.FindMatchingVehicles(f.FamilyId).Count()
            });

            return Json(result);
        }

        // GET /Families/Manage – admin-tabben: lista familjer och skapa nya
        [HttpGet]
        public IActionResult Manage()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(BuildPage(new FamilyForm()));
        }

        // POST /Families/Create – skapar en bil- eller skåpbilsfamilj via VehicleFamilyService
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FamilyForm form)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            if (!ModelState.IsValid)
                return View("Manage", BuildPage(form));

            try
            {
                // Kolla bilden först, så familjen inte skapas om den är ogiltig
                if (form.Image is { Length: > 0 })
                    _images.Validate(form.Image);

                VehicleFamily created;
                if (form.Type == VehicleType.Van)
                    created = _familyService.CreateVanFamily(form.Name.Trim(), form.Description ?? "", form.TransmissionType,
                        form.VanCategory, form.MinCargoVolumeM3, form.MinPayloadKg, form.MinPassengerCapacity);
                else
                    created = _familyService.CreatePassengerCarFamily(form.Name.Trim(), form.Description ?? "", form.TransmissionType,
                        form.Comfort, form.CarCategory);

                if (form.Image is { Length: > 0 })
                    created.ImagePath = await _images.SaveAsync(form.Image, "families");

                // Koppla redan befintliga bilar till den nya familjen
                _assignmentService.AutoAssignAllVehicles();

                TempData["Success"] = $"Family \"{form.Name.Trim()}\" was created.";
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Manage", BuildPage(form));
            }
        }

        // ---- Detaljvy ----

        // GET /Families/Details/{id} – egenskaper och bilarna som ingår
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            VehicleFamily? f = _familyRepository.GetById(id);
            if (f is null)
                return NotFound();

            var car = f as PassengerCarFamily;
            var van = f as VanFamily;

            return View(new FamilyDetailsViewModel
            {
                FamilyId = f.FamilyId,
                Name = f.Name,
                Description = f.Description,
                ImagePath = f.ImagePath,
                Kind = f is VanFamily ? "Van" : "Car",
                Transmission = f.TransmissionType.ToString(),
                Category = car?.CategoryType.ToString() ?? van!.CategoryType.ToString(),
                Comfort = car?.ComfortType.ToString(),
                MinCargoVolumeM3 = van?.MinCargoVolumeM3,
                MinPayloadKg = van?.MinPayloadKg,
                MinPassengerCapacity = van?.MinPassengerCapacity,
                Vehicles = _assignmentService.FindMatchingVehicles(id)
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
                    }).ToList()
            });
        }

        // ---- Redigera / radera ----

        // GET /Families/Edit/{id}
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            VehicleFamily? f = _familyRepository.GetById(id);
            if (f is null)
                return NotFound();

            var van = f as VanFamily;
            return View(new FamilyEditForm
            {
                FamilyId = id,
                Kind = f is VanFamily ? "Van" : "Car",
                Name = f.Name,
                Description = f.Description,
                MinCargoVolumeM3 = van?.MinCargoVolumeM3,
                MinPayloadKg = van?.MinPayloadKg,
                MinPassengerCapacity = van?.MinPassengerCapacity,
                CurrentImagePath = f.ImagePath
            });
        }

        // POST /Families/Edit – typ, transmission och kategori styr matchningen och kan inte ändras
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FamilyEditForm form)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            VehicleFamily? f = _familyRepository.GetById(form.FamilyId);
            if (f is null)
                return NotFound();

            form.Kind = f is VanFamily ? "Van" : "Car";
            form.CurrentImagePath = f.ImagePath;

            if (!ModelState.IsValid)
                return View(form);

            try
            {
                if (string.IsNullOrWhiteSpace(form.Name))
                    throw new ArgumentException("Name is required.");

                if (form.Image is { Length: > 0 })
                    _images.Validate(form.Image);

                f.UpdateDescriptive(form.Name.Trim(), form.Description ?? "");

                if (f is VanFamily van)
                {
                    van.UpdateMinimums(form.MinCargoVolumeM3, form.MinPayloadKg, form.MinPassengerCapacity);
                    _assignmentService.ReassignAllVehicles();   // minimikraven styr matchningen
                }

                if (form.Image is { Length: > 0 })
                    f.ImagePath = await _images.SaveAsync(form.Image, "families");

                TempData["Success"] = $"Family \"{f.Name}\" was updated.";
                return RedirectToAction(nameof(Details), new { id = f.FamilyId });
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                ModelState.AddModelError("", ex.Message);
                return View(form);
            }
        }

        // POST /Families/Delete – blockeras om familjen har bilar eller bokningar
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            VehicleFamily? f = _familyRepository.GetById(id);
            if (f is null)
            {
                TempData["Error"] = "Family not found.";
                return RedirectToAction(nameof(Manage));
            }

            int vehicles = _assignmentService.FindMatchingVehicles(id).Count();
            int bookings = _bookings.GetAll().Count(b => b.VehicleFamilyId == id);

            if (vehicles > 0)
                TempData["Error"] = $"Family \"{f.Name}\" has {vehicles} vehicle(s) and cannot be deleted. Remove or move the vehicles first.";
            else if (bookings > 0)
                TempData["Error"] = $"Family \"{f.Name}\" has {bookings} booking(s) and cannot be deleted.";
            else
            {
                _familyRepository.Remove(id);
                TempData["Success"] = $"Family \"{f.Name}\" was deleted.";
            }

            return RedirectToAction(nameof(Manage));
        }

        private FamilyPageViewModel BuildPage(FamilyForm form) => new()
        {
            Form = form,
            Families = _familyRepository.GetAll().Select(f => new FamilyRowViewModel
            {
                FamilyId = f.FamilyId,
                Name = f.Name,
                Description = f.Description,
                ImagePath = f.ImagePath,
                Kind = f is VanFamily ? "Van" : "Car",
                Transmission = f.TransmissionType.ToString(),
                VehicleCount = _assignmentService.FindMatchingVehicles(f.FamilyId).Count()
            }).ToList()
        };

        // Samma inloggnings- och rollkontroll som övriga adminsidor (personal)
        private IActionResult? GetStaff(out IBookingManager staff)
        {
            staff = null!;
            User? user = _session.CurrentUser;

            if (user is null)
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

            if (user is not IBookingManager manager)
                return StatusCode(403);

            staff = manager;
            return null;
        }

        // GET /Families/Availability?familyId=...&start=...&end=...
        [HttpGet]
        public IActionResult Availability(Guid familyId, DateTime start, DateTime end)
        {
            if (_familyRepository.GetById(familyId) == null)
                return NotFound();

            return Json(new
            {
                familyId,
                start,
                end,
                available = _assignmentService.IsFamilyAvailable(familyId, start, end)
            });
        }
    }
}
