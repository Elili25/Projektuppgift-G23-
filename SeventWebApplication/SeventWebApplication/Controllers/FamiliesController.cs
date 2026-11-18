using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
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

        public FamiliesController(
            VehicleFamilyRepository familyRepository,
            VehicleAssignmentService assignmentService,
            VehicleFamilyService familyService,
            UserSession session,
            ImageUploadService images)
        {
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

        private FamilyPageViewModel BuildPage(FamilyForm form) => new()
        {
            Form = form,
            Families = _familyRepository.GetAll().Select(f => new FamilyRowViewModel
            {
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
