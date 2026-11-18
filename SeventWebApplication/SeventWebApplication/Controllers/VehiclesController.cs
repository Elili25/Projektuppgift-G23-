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

        private BrowseVehiclesViewModel Build(bool isVans, string? category)
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
