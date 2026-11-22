using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Controllers
{
    // Admin: dokumentation av designbeslut. Sidorna är statiska och bara för personal.
    public class DocumentationController : StaffController
    {
        public DocumentationController(UserSession session) : base(session)
        {
        }

        // GET /Documentation/PickupReturn – uthämtning och återlämning utanför bokningsfönstret
        [HttpGet]
        public IActionResult PickupReturn()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View();
        }
    }
}
