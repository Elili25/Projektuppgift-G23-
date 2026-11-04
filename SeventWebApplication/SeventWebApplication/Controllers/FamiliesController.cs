using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Gör fordonsfamiljerna nåbara från webben: lista familjer och kolla om en familj är ledig.
    public class FamiliesController : Controller
    {
        private readonly VehicleFamilyRepository _familyRepository;
        private readonly VehicleAssignmentService _assignmentService;

        public FamiliesController(
            VehicleFamilyRepository familyRepository,
            VehicleAssignmentService assignmentService)
        {
            _familyRepository = familyRepository;
            _assignmentService = assignmentService;
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
