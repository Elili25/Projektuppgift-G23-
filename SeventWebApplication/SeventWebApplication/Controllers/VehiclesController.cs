using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Guest;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Publik sida där man bläddrar bland fordonsfamiljerna och fordonen (/Vehicles).
    // kind = cars | vans, category = t.ex. SUV, Sedan, CargoVan (båda valfria).
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

        [HttpGet]
        public IActionResult Index(string? kind, string? category)
        {
            bool carsOnly = string.Equals(kind, "cars", StringComparison.OrdinalIgnoreCase);
            bool vansOnly = string.Equals(kind, "vans", StringComparison.OrdinalIgnoreCase);

            var cards = new List<FamilyCardViewModel>();

            foreach (var family in _families.GetAll())
            {
                string kindLabel, categoryLabel, categoryKey;
                string? comfort = null;

                switch (family)
                {
                    case PassengerCarFamily car when !vansOnly:
                        kindLabel = "Passenger car";
                        categoryKey = car.CategoryType.ToString();
                        categoryLabel = car.CategoryType == PassengerCarCategoryType.StationWagon ? "Station wagon" : categoryKey;
                        comfort = car.ComfortType.ToString();
                        break;

                    case VanFamily van when !carsOnly:
                        kindLabel = "Van";
                        categoryKey = van.CategoryType.ToString();
                        categoryLabel = van.CategoryType == VanCategoryType.CargoVan ? "Cargo van" : "Passenger van";
                        break;

                    default:
                        continue;
                }

                if (!string.IsNullOrEmpty(category) &&
                    !string.Equals(category, categoryKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                var vehicles = _assignment.FindMatchingVehicles(family.FamilyId).ToList();

                cards.Add(new FamilyCardViewModel
                {
                    Name = family.Name,
                    Description = family.Description,
                    Kind = kindLabel,
                    Category = categoryLabel,
                    Transmission = family.TransmissionType.ToString(),
                    Comfort = comfort,
                    VehicleCount = vehicles.Count,
                    Vehicles = vehicles
                        .Take(4)
                        .Select(v => $"{v.Model.Brand.BrandName} {v.Model.ModelName} · {v.Year} · {v.PassengerCapacity} seats")
                        .ToList()
                });
            }

            string title = (carsOnly, vansOnly, string.IsNullOrEmpty(category)) switch
            {
                (_, _, false) => cards.FirstOrDefault()?.Category ?? category!,
                (true, _, _) => "Passenger cars",
                (_, true, _) => "Vans",
                _ => "All vehicles"
            };

            return View(new BrowseVehiclesViewModel { Title = title, Families = cards.OrderBy(c => c.Kind).ThenBy(c => c.Name).ToList() });
        }
    }
}
