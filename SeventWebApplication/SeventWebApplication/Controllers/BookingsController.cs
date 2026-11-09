using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.BookingValidation;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;
using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Enums;

namespace SeventWebApplication.Controllers
{
    // Sidan där man kollar på bokningar (/Bookings).
    // Controllern bestämmer INTE vilka bokningar som syns – det gör användarens
    // egen Visibility via BookingViewerService. Här fixar vi bara inloggningskollen,
    // filter/sök och gör om bokningarna till rader som vyn kan visa.
    public class BookingsController : Controller
    {
        private readonly UserSession _session;
        private readonly BookingViewerService _viewer;
        private readonly IUserRepository _users;
        private readonly BookingCreationService _creationService;
        private readonly FleetManager _fleetManager;
        private readonly VehicleFamilyRepository _families;

        public BookingsController(
            UserSession session,
            BookingViewerService viewer,
            IUserRepository users,
            BookingCreationService creationService,
            FleetManager fleetManager,
            VehicleFamilyRepository families)
        {
            _session = session;
            _viewer = viewer;
            _users = users;
            _creationService = creationService;
            _fleetManager = fleetManager;
            _families = families;
        }

        // status = "pending" / "confirmed" / "active" / "completed" / "cancelled" (eller inget = alla)
        // search = fritext, bara för personal
        [HttpGet]
        public IActionResult Index(string? status, string? search)
        {
            User? user = _session.CurrentUser;

            // Inte inloggad? Skicka till inloggningen och sen tillbaka hit
            if (user is null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Bookings") });

            // Alla roller vi har nu kan titta på bokningar, men om nån roll inte kan det sen
            // så stoppas den här istället för att krascha.
            // (StatusCode(403) istället för Forbid(), Forbid kräver ASP.NET:s inbyggda inloggning som vi inte kör)
            // Detta borde aldrig hända, men det är bra att ha en check här ifall nån roll tas bort eller läggs till i framtiden.
            if (user is not IBookingViewer viewer)
                return StatusCode(403);

            bool isStaff = user is IBookingManager;
            DateTime now = DateTime.Now;

            // Det är här det bestäms vilka bokningar som syns för den här användaren.
            // Vi får tillbaka alla bokningar som användaren kan se i en lista, och gör om dem till rader som sidan kan visa.
            List<BookingRowViewModel> rows = _viewer.ViewBookings(viewer)
                .Select(b => BookingRowViewModel.From(b, _users, now))
                .ToList();

            // Sökfunktion för personal och admins, funkar med kundnamn, mail, regnr, bil eller boknings-id
            string? trimmedSearch = isStaff ? search?.Trim() : null;
            if (!string.IsNullOrEmpty(trimmedSearch))
            {
                rows = rows.Where(r =>
                        r.CustomerName.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase) ||
                        r.CustomerMail.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase) ||
                        r.RegNr.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase) ||
                        r.Vehicle.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase) ||
                        r.BookingId.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Räkna per status INNAN vi filtrerar på status, så flikarna visar rätt siffror
            BookingStatus? selected = ParseStatus(status);

            var model = new BookingListViewModel
            {
                IsStaff = isStaff,
                Search = trimmedSearch,
                SelectedStatus = selected,
                CountAll = rows.Count,
                CountPending = rows.Count(r => r.Status == BookingStatus.Pending),
                CountConfirmed = rows.Count(r => r.Status == BookingStatus.Confirmed),
                CountActive = rows.Count(r => r.Status == BookingStatus.Active),
                CountCompleted = rows.Count(r => r.Status == BookingStatus.Completed),
                CountCancelled = rows.Count(r => r.Status == BookingStatus.Cancelled),
                Bookings = rows
                    .Where(r => selected is null || r.Status == selected)
                    .OrderBy(r => (int)r.Status)
                    .ThenBy(r => r.Start)
                    .ToList()
            };

            return View(model);
        }

        // Här hanteras statusfiltren: gör om texten från URL:en ("pending", "confirmed" ...) till BookingStatus,
        // eller null för inget filter aka visa alla. IsDefined stoppar siffror som "7" som inte är en riktig status.
        private static BookingStatus? ParseStatus(string? status) =>
            Enum.TryParse(status, ignoreCase: true, out BookingStatus parsed) && Enum.IsDefined(parsed)
                ? parsed
                : null;

        // ---- Ny bokning (/Bookings/Create) ----
        // Controllern bestämmer INTE åt vem man får boka eller om bokningen godkänns direkt –
        // det gör användarens CreationStrategy inne i BookingCreationService.
        // Här kollar vi bara inloggning, att rollen får boka alls, och bygger listorna till formuläret.

        [HttpGet]
        public IActionResult Create()
        {
            if (GetCreator(out _) is IActionResult denied)
                return denied;

            return View(BuildCreateModel(new CreateBookingForm(), new List<ValidationError>()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind(Prefix = "Form")] CreateBookingForm form)
        {
            if (GetCreator(out IBookingCreator creator) is IActionResult denied)
                return denied;

            BookingResult result = _creationService.CreateBooking(creator, form.ToRequest());

            if (result.Success && result.CreatedBooking is { } booking)
            {
                string shortId = booking.BookingId[..8].ToUpper();
                TempData["Success"] = booking.Status == BookingStatus.Confirmed
                    ? $"Bokning #{shortId} är skapad och bekräftad."
                    : $"Bokningsförfrågan #{shortId} är skickad och väntar på godkännande.";
                return RedirectToAction(nameof(Index));
            }

            // Visa felen och behåll det man fyllt i, så man inte behöver börja om
            return View(BuildCreateModel(form, result.Errors));
        }

        // Samma sak som formuläret men med JSON (POST /Bookings/CreateJson), om startsidans JS-modul vill boka direkt
        [HttpPost]
        public IActionResult CreateJson([FromBody] BookingRequest request)
        {
            if (_session.CurrentUser is not { } user)
                return Unauthorized();

            if (user is not IBookingCreator creator)
                return StatusCode(403);

            BookingResult result = _creationService.CreateBooking(creator, request);

            // Skickar inte hela Booking-objektet (bilen har många nästlade objekt), bara det klienten behöver
            var response = new
            {
                result.Success,
                BookingId = result.CreatedBooking?.BookingId,
                Status = result.CreatedBooking?.Status.ToString(),
                Errors = result.Errors
            };

            return result.Success ? Ok(response) : BadRequest(response);
        }

        // --- Hjälpmetoder för Create ---

        // Inloggad och får skapa bokningar? Annars login-sidan eller 403
        private IActionResult? GetCreator(out IBookingCreator creator)
        {
            creator = null!;
            User? user = _session.CurrentUser;

            if (user is null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", "Bookings") });

            if (user is not IBookingCreator bookingCreator)
                return StatusCode(403);

            creator = bookingCreator;
            return null;
        }

        private CreateBookingViewModel BuildCreateModel(CreateBookingForm form, List<ValidationError> errors)
        {
            // Samma "är personal"-koll som bokningslistan och menyn använder
            User? currentUser = _session.CurrentUser;
            bool isStaff = currentUser is IBookingManager;

            return new CreateBookingViewModel
            {
                Form = form,
                IsStaff = isStaff,
                Errors = errors,

                // Personal får boka åt alla aktiva användare: kunder, annan personal och sig själv.
                // Den inloggade hamnar först så att "boka åt mig själv" är närmast till hands.
                Customers = isStaff
                    ? _users.GetAll()
                        .Where(u => u.IsActive)
                        .OrderBy(u => u.UserId != currentUser!.UserId)
                        .ThenBy(u => u.Role != Role.Customer)
                        .ThenBy(u => u.Name)
                        .Select(u => new BookingOption(
                            u.UserId.ToString(),
                            u.UserId == currentUser!.UserId ? $"{u.Name} (du)" : u.Name,
                            u.Role == Role.Customer ? u.Mail : $"{u.Mail}, {UsersController.RoleText(u.Role)}"))
                        .ToList()
                    : new List<BookingOption>(),

                Families = _families.GetAll()
                    .OrderBy(f => f.Name)
                    .Select(f => new BookingOption(f.FamilyId.ToString(), f.Name, f.Description))
                    .ToList(),

                // Bilar som är ur drift går inte att boka, så de visas inte
                Vehicles = _fleetManager.GetAllVehicles()
                    .Where(v => v.IsOperational)
                    .OrderBy(v => v.Model.Brand.BrandName).ThenBy(v => v.Model.ModelName)
                    .Select(v => new BookingOption(
                        v.VehicleId.ToString(),
                        $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                        v.RegistrationPlate.RegNr))
                    .ToList(),

                Addons = GetAddonOptions()
            };
        }

        // Ett val per AddonType. Nyckeln är enumnamnet i gemener ("gps", "childseat"...), samma som AddonFactory använder.
        // Namn, maxantal och pris hämtas från själva tillägget, så formuläret följer med om de ändras.
        private static List<AddonOption> GetAddonOptions()
        {
            var options = new List<AddonOption>();

            foreach (AddonType type in Enum.GetValues<AddonType>())
            {
                string key = type.ToString().ToLowerInvariant();
                try
                {
                    var addon = AddonFactory.Create(key, 1);
                    options.Add(new AddonOption(key, addon.DisplayName, addon.MaxLimit, addon.GetPrice()));
                }
                catch (ArgumentException)
                {
                    // Enum-värde utan matchande klass, hoppa över istället för att krascha sidan
                }
            }

            return options;
        }
    }
}
