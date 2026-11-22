using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Controllers
{
    // Gemensam bas för adminsidorna (Fleet, BookingManagement, Returns).
    // Bara personal (IBookingManager = Employee och Admin) kommer in, samma upplägg som UsersController.GetAdmin.
    public abstract class StaffController : Controller
    {
        protected readonly UserSession Session;

        protected StaffController(UserSession session)
        {
            Session = session;
        }

        // Returnerar null om allt är ok, annars ett svar (inloggning eller 403) som skickas tillbaka direkt.
        protected IActionResult? GetStaff(out IBookingManager staff)
        {
            staff = null!;
            User? user = Session.CurrentUser;

            if (user is null)
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

            if (user is not IBookingManager manager)
                return StatusCode(403);

            staff = manager;
            return null;
        }

        // Kör en åtgärd, visar ett meddelande och går tillbaka till angiven sida.
        // Reglerna kastar InvalidOperationException/ArgumentException/UnauthorizedAccessException, de visas för användaren.
        protected IActionResult RunAndRedirect(Action<IBookingManager> action, string doneText, string redirectAction, string? redirectController = null)
        {
            if (GetStaff(out IBookingManager staff) is IActionResult denied)
                return denied;

            try
            {
                action(staff);
                TempData["Success"] = doneText;
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or UnauthorizedAccessException)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(redirectAction, redirectController);
        }
    }
}
