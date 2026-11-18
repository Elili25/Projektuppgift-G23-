using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Guest;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Publik sida där man bläddrar bland fordonsfamiljerna. Personbilar och skåpbilar har varsin sida:
    //   /Vehicles/Cars?category=SUV   och   /Vehicles/Vans?category=CargoVan   (category är valfri)
    // Öppen för alla, ingen inloggning krävs.
    public class VehiclesController : Controller
    {
        private readonly VehicleFamilyRepository _families;
        private readonly VehicleAssignmentService _assignment;

        public VehiclesController(VehicleFamilyRepository families, VehicleAssignmentService assignment)
        {
            _families = families;
            _assignment = assignment;
        }

        // /Vehicles (och äldre länkar med ?category=) skickas vidare till rätt sida
        [HttpGet]
        public IActionResult Index(string? category)
        {
            if (category is "CargoVan" or "PassengerVan" or "Van")
                return RedirectToAction(nameof(Vans), new { category = category == "Van" ? null : category });

            bool isCarCategory = Enum.TryParse<PassengerCarCategoryType>(category, true, out _);
            return RedirectToAction(nameof(Cars), new { category = isCarCategory ? category : null });
        }

        [HttpGet]
        public IActionResult Cars(string? category) => View("Browse", Build(isVans: false, category));

        [HttpGet]
        public IActionResult Vans(string? category) => View("Browse", Build(isVans: true, category));

        // GET /Vehicles/Available?start=2026-12-01&end=2026-12-05&type=cars|vans (type är valfri)
        // Startsidans sökruta anropar den med fetch och får tillbaka bara resultatdelen (partial).
        // Utan fetch visas samma resultat som en egen sida.
        [HttpGet]
        public IActionResult Available(DateTime? start, DateTime? end, string? type)
        {
            AvailabilityResultViewModel model;

            if (start is null || end is null)
            {
                model = new AvailabilityResultViewModel { Error = "Choose both a pickup and a return date." };
            }
            else if (start.Value.Date < DateTime.Today)
            {
                model = new AvailabilityResultViewModel { Start = start, End = end, Error = "The pickup date cannot be in the past." };
            }
            else if (end.Value.Date <= start.Value.Date)
            {
                model = new AvailabilityResultViewModel { Start = start, End = end, Error = "The return date must be after the pickup date." };
            }
            else
            {
                // Samma standardtider som bokningsformuläret (kl 10)
                DateTime from = start.Value.Date.AddHours(10);
                DateTime to = end.Value.Date.AddHours(10);

                bool carsOnly = string.Equals(type, "cars", StringComparison.OrdinalIgnoreCase);
                bool vansOnly = string.Equals(type, "vans", StringComparison.OrdinalIgnoreCase);

                var families = new List<FamilyCardViewModel>();
                if (!vansOnly) families.AddRange(Build(isVans: false, null, from, to).Families);
                if (!carsOnly) families.AddRange(Build(isVans: true, null, from, to).Families);

                model = new AvailabilityResultViewModel { Start = start, End = end, Families = families };
            }

            // fetch skickar den här headern, då räcker själva resultatdelen
            if (Request.Headers["X-Requested-With"] == "fetch")
                return PartialView("_AvailableResults", model);

            return View("Available", model);
        }

        private BrowseVehiclesViewModel Build(bool isVans, string? category, DateTime? from = null, DateTime? to = null)
        {
            var cards = new List<FamilyCardViewModel>();

            foreach (var family in _families.GetAll())
            {
                string categoryKey, categoryLabel;

                if (family is PassengerCarFamily car && !isVans)
                {
                    categoryKey = car.CategoryType.ToString();
                    categoryLabel = car.CategoryType == PassengerCarCategoryType.StationWagon ? "Station wagon" : categoryKey;
                }
                else if (family is VanFamily van && isVans)
                {
                    categoryKey = van.CategoryType.ToString();
                    categoryLabel = van.CategoryType == VanCategoryType.CargoVan ? "Cargo van" : "Passenger van";
                }
                else
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(category) &&
                    !string.Equals(category, categoryKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Sökning med datum: bara klasser som har en ledig bil under perioden
                if (from is DateTime f && to is DateTime t && !_assignment.IsFamilyAvailable(family.FamilyId, f, t))
                    continue;

                // Ett exempelfordon ur familjen ger specifikationerna och namnet ("Volvo V60 or similar")
                Vehicle? example = _assignment.FindMatchingVehicles(family.FamilyId).FirstOrDefault();

                var tags = new List<string> { family.TransmissionType.ToString() };
                if (family is PassengerCarFamily pc)
                    tags.Add(pc.ComfortType.ToString());

                var specs = new List<(string, string)> { ("Gearbox", family.TransmissionType.ToString()) };
                if (family is PassengerCarFamily comfortFamily)
                    specs.Add(("Comfort", comfortFamily.ComfortType.ToString()));

                if (example != null)
                {
                    specs.Add(("Seats", example.PassengerCapacity.ToString()));
                    if (example is Van v)
                    {
                        specs.Add(("Cargo", $"{v.CargoCapacityM3:0.#} m³"));
                        specs.Add(("Payload", $"{v.MaxPayloadKg:0} kg"));
                    }
                    else
                    {
                        specs.Add(("Luggage", $"{example.LuggageCapacity:0} L"));
                    }
                }

                cards.Add(new FamilyCardViewModel
                {
                    FamilyId = family.FamilyId,
                    Name = family.Name,
                    Description = family.Description,
                    Category = categoryLabel,
                    Tags = tags,
                    Specs = specs,
                    Example = example is null ? null : $"{example.Model.Brand.BrandName} {example.Model.ModelName}",
                    ImagePath = family.ImagePath
                        ?? CategoryImage(categoryKey)
                        ?? example?.ImagePath
                });
            }

            return new BrowseVehiclesViewModel
            {
                IsVans = isVans,
                Category = category,
                Families = cards.OrderBy(c => c.Category).ThenBy(c => c.Name).ToList()
            };
        }

        // Bilderna ligger i wwwroot/lib/Images/FeaturedVehicleImages, en per biltyp
        private static string? CategoryImage(string categoryKey) => categoryKey switch
        {
            "Sedan" => "/lib/Images/FeaturedVehicleImages/PassengerCar/VehicleFamily-Luxury-Sedan-FeaturedImage.png",
            "SUV" => "/lib/Images/FeaturedVehicleImages/PassengerCar/VehicleFamily-Premium-SUV-FeaturedImage.png",
            "Hatchback" => "/lib/Images/FeaturedVehicleImages/PassengerCar/VehicleFamily-Standard-Hatchback-FeaturedImage.png",
            "StationWagon" => "/lib/Images/FeaturedVehicleImages/PassengerCar/VehicleFamily-Economy-StationWagon-FeaturedImage.png",
            "CargoVan" => "/lib/Images/FeaturedVehicleImages/Van/VehicleFamily-Cargo-Van-FeaturedImage.png",
            "PassengerVan" => "/lib/Images/FeaturedVehicleImages/Van/VehicleFamily-PassengerVan-FeaturedImage.png",
            _ => null
        };
    }
}
