using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Guest;
using SeventWebApplication.Models.Pricing;
using SeventWebApplication.Models.VehicleNS;
using SeventWebApplication.Models.VehicleNS.Engine;

namespace SeventWebApplication.Controllers
{
    // Publik sida där man bläddrar bland fordonsfamiljerna. Personbilar och skåpbilar har varsin sida:
    //   /Vehicles/Cars?category=SUV   och   /Vehicles/Vans?category=CargoVan   (category är valfri)
    // Öppen för alla, ingen inloggning krävs.
    public class VehiclesController : Controller
    {
        private readonly VehicleFamilyRepository _families;
        private readonly VehicleAssignmentService _assignment;
        private readonly PricingServices _pricing;

        public VehiclesController(VehicleFamilyRepository families, VehicleAssignmentService assignment, PricingServices pricing)
        {
            _families = families;
            _assignment = assignment;
            _pricing = pricing;
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

        // GET /Vehicles/Family/{id} – produktsida för en fordonsfamilj
        [HttpGet]
        public IActionResult Family(Guid id)
        {
            VehicleFamily? family = _families.GetById(id);
            if (family is null)
                return NotFound();

            return View(BuildFamilyPage(family));
        }

        [HttpGet]
        public IActionResult Cars(string? category) => View("Browse", Build(isVans: false, category));

        [HttpGet]
        public IActionResult Vans(string? category) => View("Browse", Build(isVans: true, category));

        // GET /Vehicles/Available?start=2026-12-01&end=2026-12-05&family=<id>  (family och type=cars|vans är valfria)
        // Startsidans sökruta anropar den med fetch och får tillbaka bara resultatdelen (partial).
        // Utan fetch visas samma resultat som en egen sida.
        [HttpGet]
        public IActionResult Available(DateTime? start, DateTime? end, string? type, Guid? family)
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

                // Vald fordonsfamilj i sökrutan: visa bara den
                if (family is Guid familyId)
                    families = families.Where(f => f.FamilyId == familyId).ToList();

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
                        specs.Add(("Cargo", FormattableString.Invariant($"{v.CargoCapacityM3:0.#} m³")));
                        specs.Add(("Payload", FormattableString.Invariant($"{v.MaxPayloadKg:0} kg")));
                    }
                    else
                    {
                        specs.Add(("Luggage", FormattableString.Invariant($"{example.LuggageCapacity:0} L")));
                    }
                }

                cards.Add(new FamilyCardViewModel
                {
                    FamilyId = family.FamilyId,
                    Name = family.Name,
                    Description = family.Description,
                    IsVan = isVans,
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

        // Bygger produktsidan. Specifikationerna är en lista av grupper med rader (SpecGroup), så en ny spec
        // läggs bara till här som en rad i rätt grupp, vyn Family.cshtml ritar ut allt som finns.
        // Värden sammanfattas över alla fordon i familjen (t.ex. "361–550 L" om bilarna skiljer sig åt).
        private FamilyPageViewModel BuildFamilyPage(VehicleFamily family)
        {
            var vehicles = _assignment.FindMatchingVehicles(family.FamilyId).ToList();
            var cars = vehicles.OfType<Car>().ToList();
            var vans = vehicles.OfType<Van>().ToList();

            bool isVan = family is VanFamily;
            string categoryKey = family switch
            {
                PassengerCarFamily pc => pc.CategoryType.ToString(),
                VanFamily vf => vf.CategoryType.ToString(),
                _ => ""
            };
            string categoryLabel = categoryKey switch
            {
                "StationWagon" => "Station wagon",
                "CargoVan" => "Cargo van",
                "PassengerVan" => "Passenger van",
                var other => other
            };

            // ---- Specifikationer ----
            var overview = new SpecGroup { Title = "Overview" };
            overview.Items.Add(("Type", isVan ? "Van" : "Passenger car"));
            overview.Items.Add(("Category", categoryLabel));
            if (family is PassengerCarFamily comfortFamily)
                overview.Items.Add(("Comfort level", comfortFamily.ComfortType.ToString()));
            overview.Items.Add(("Gearbox", family.TransmissionType.ToString()));
            if (vehicles.Count > 0)
                overview.Items.Add(("Model years", Range(vehicles.Select(v => (double)v.Year), "")));

            var capacity = new SpecGroup { Title = "Capacity" };
            if (vehicles.Count > 0)
                capacity.Items.Add(("Seats", Range(vehicles.Select(v => (double)v.PassengerCapacity), "")));
            if (cars.Count > 0)
                capacity.Items.Add(("Luggage space", Range(cars.Select(c => (double)c.LuggageCapacity), " L")));
            if (vans.Count > 0)
            {
                capacity.Items.Add(("Cargo volume", Range(vans.Select(v => v.CargoCapacityM3), " m³")));
                capacity.Items.Add(("Max payload", Range(vans.Select(v => v.MaxPayloadKg), " kg")));
            }
            if (family is VanFamily requirements)
            {
                if (requirements.MinCargoVolumeM3 is double minCargo) capacity.Items.Add(("Guaranteed cargo volume", FormattableString.Invariant($"At least {minCargo:0.#} m³")));
                if (requirements.MinPayloadKg is double minPayload) capacity.Items.Add(("Guaranteed payload", FormattableString.Invariant($"At least {minPayload:0} kg")));
                if (requirements.MinPassengerCapacity is int minSeats) capacity.Items.Add(("Guaranteed seats", $"At least {minSeats}"));
            }

            var engine = new SpecGroup { Title = "Engine & efficiency" };
            if (vehicles.Count > 0)
            {
                engine.Items.Add(("Fuel", string.Join(", ", vehicles.Select(v => v.Engine.FuelType.ToString()).Distinct())));
                engine.Items.Add(("Power", Range(vehicles.Select(v => (double)v.Engine.HorsePower), " hp")));

                var electric = vehicles.Select(v => v.Engine).OfType<ElectricEngine>().ToList();
                if (electric.Count > 0)
                {
                    engine.Items.Add(("Battery", Range(electric.Select(e => (double)e.BatteryCapacityKwh), " kWh")));
                    engine.Items.Add(("Range", Range(electric.Select(e => (double)e.RangeKm), " km")));
                }
            }

            // Tomma grupper visas inte
            var groups = new[] { overview, capacity, engine }.Where(g => g.Items.Count > 0).ToList();

            // ---- Nyckelfakta överst ----
            var highlights = new List<(string, string)> { ("Gearbox", family.TransmissionType.ToString()) };
            if (vehicles.Count > 0)
            {
                highlights.Insert(0, ("Seats", Range(vehicles.Select(v => (double)v.PassengerCapacity), "")));
                highlights.Add(("Fuel", string.Join(" / ", vehicles.Select(v => v.Engine.FuelType.ToString()).Distinct())));
            }

            // ---- Pris: det lägsta dagspriset i familjen ----
            decimal? fromPrice = null;
            foreach (var v in vehicles)
            {
                try
                {
                    decimal daily = _pricing.CalculateBaseRate(v, 1);
                    fromPrice = fromPrice is null ? daily : Math.Min(fromPrice.Value, daily);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Fordon utan prissättning hoppas över
                }
            }

            return new FamilyPageViewModel
            {
                FamilyId = family.FamilyId,
                Name = family.Name,
                Description = family.Description,
                IsVan = isVan,
                Category = categoryLabel,
                ImagePath = family.ImagePath ?? CategoryImage(categoryKey) ?? vehicles.Select(v => v.ImagePath).FirstOrDefault(p => !string.IsNullOrEmpty(p)),
                FromDailyPrice = fromPrice,
                Tags = new List<string> { categoryLabel, family.TransmissionType.ToString() }
                    .Concat(family is PassengerCarFamily c ? new[] { c.ComfortType.ToString() } : Array.Empty<string>())
                    .ToList(),
                Highlights = highlights,
                SpecGroups = groups,
                Models = vehicles
                    .OrderBy(v => v.Model.Brand.BrandName).ThenBy(v => v.Model.ModelName)
                    .Select(v => new FamilyModelItem
                    {
                        Name = $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                        Details = $"{v.Year} · {v.Engine.HorsePower} hp · {v.Engine.FuelType} · {v.PassengerCapacity} seats"
                    })
                    .ToList()
            };
        }

        // "5" om alla är lika, annars "361–550"; unit läggs till efter, t.ex. " L"
        private static string Range(IEnumerable<double> values, string unit)
        {
            var list = values.ToList();
            double min = list.Min(), max = list.Max();
            return min == max
                ? FormattableString.Invariant($"{min:0.#}{unit}")
                : FormattableString.Invariant($"{min:0.#}–{max:0.#}{unit}");
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
