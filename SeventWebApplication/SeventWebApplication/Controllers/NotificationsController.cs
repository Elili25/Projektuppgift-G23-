using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Notifications;
using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Controllers
{
    // Ger popup-rutan i frontend inloggad användares notiser (skapade av NotificationService, som är observer på bokningshändelser).
    public class NotificationsController : Controller
    {
        private readonly UserSession _session;
        private readonly NotificationService _notifications;

        public NotificationsController(UserSession session, NotificationService notifications)
        {
            _session = session;
            _notifications = notifications;
        }

        // GET /Notifications/Latest – JSON, pollas av _NotificationToasts
        [HttpGet]
        public IActionResult Latest()
        {
            User? user = _session.CurrentUser;
            if (user is null)
                return Json(new { loggedIn = false, items = Array.Empty<object>() });

            var items = _notifications.GetNotificationsForUser(user.UserId)
                .Select(n => new { id = n.NotificationId, message = n.Message, type = n.Type.ToString() });

            return Json(new { loggedIn = true, items });
        }
    }
}
