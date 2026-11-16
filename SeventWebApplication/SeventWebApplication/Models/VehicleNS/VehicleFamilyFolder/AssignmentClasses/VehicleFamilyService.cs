using SeventWebApplication.Models.Pricing.PricingCategories;
using SeventWebApplication.ViewModels;

namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleFamilyService
    {
        private readonly VehicleFamilyFactory _familyFactory;
        private readonly VehicleFamilyRepository _familyRepository;
        private readonly VehicleFamilyValidator _validator;
        private readonly VehicleAssignmentService _assignmentService;
        private readonly CarRateCatalog _carRates;
        private readonly VanRateCatalog _vanRates;

        // Enumen har ordningen Economy, Standard, Luxury, Premium, så panelerna sorteras explicit
        private static readonly VehicleComfortType[] TierOrder =
        {
            VehicleComfortType.Economy,
            VehicleComfortType.Standard,
            VehicleComfortType.Premium,
            VehicleComfortType.Luxury
        };

        // Ordningen personbilskategorierna visas i (enumen har en annan ordning)
        private static readonly PassengerCarCategoryType[] CarCategoryOrder =
        {
            PassengerCarCategoryType.Hatchback,
            PassengerCarCategoryType.Sedan,
            PassengerCarCategoryType.StationWagon,
            PassengerCarCategoryType.SUV
        };

        // Ordningen skåpbilarna visas i
        private static readonly VanCategoryType[] VanCategoryOrder =
        {
            VanCategoryType.PassengerVan,
            VanCategoryType.CargoVan
        };

        public VehicleFamilyService(
            VehicleFamilyFactory familyFactory,
            VehicleFamilyRepository familyRepository,
            VehicleFamilyValidator validator,
            VehicleAssignmentService assignmentService,
            CarRateCatalog carRates,
            VanRateCatalog vanRates)
        {
            _familyFactory = familyFactory;
            _familyRepository = familyRepository;
            _validator = validator;
            _assignmentService = assignmentService;
            _carRates = carRates;
            _vanRates = vanRates;
        }

        public VehicleFamily CreatePassengerCarFamily(
            string name,
            string description,
            TransmissionType transmissionType,
            VehicleComfortType comfortType,
            PassengerCarCategoryType categoryType)
        {
            _validator.ValidatePassengerCar(
                name,
                transmissionType,
                comfortType,
                categoryType);

            PassengerCarFamily family =
                _familyFactory.CreatePassengerCarFamily(
                    name,
                    description,
                    transmissionType,
                    comfortType,
                    categoryType);

            _familyRepository.Add(family);

            return family;
        }

        public VehicleFamily CreateVanFamily(
            string name,
            string description,
            TransmissionType transmissionType,
            VanCategoryType categoryType,
            double? minCargoVolumeM3,
            double? minPayloadKg,
            int? minPassengerCapacity)
        {
            _validator.ValidateVan(
                name,
                transmissionType,
                categoryType,
                minCargoVolumeM3,
                minPayloadKg,
                minPassengerCapacity);

            VanFamily family =
                _familyFactory.CreateVanFamily(
                    name,
                    description,
                    transmissionType,
                    categoryType,
                    minCargoVolumeM3,
                    minPayloadKg,
                    minPassengerCapacity);

            _familyRepository.Add(family);

            return family;
        }

        // En personbilsfamilj per komfortnivå, med en representativ bil och priset från CarRateCatalog.
        // Används av FeaturedVehicles-sektionen på startsidan.
        public List<FeaturedVehicleViewModel> GetFeaturedVehicles()
        {
            return _familyRepository.GetAll()
                .OfType<PassengerCarFamily>()                 // bara personbilar, inga vans
                .GroupBy(family => family.ComfortType)        // en familj per komfortnivå
                .Select(group => group.First())
                .OrderBy(family => Array.IndexOf(TierOrder, family.ComfortType))
                .Select(family =>
                {
                    // En bil ur familjen: körbar och nyast. Null om familjen saknar bilar.
                    var car = _assignmentService.FindMatchingVehicles(family.FamilyId)
                        .Where(vehicle => vehicle.IsOperational)
                        .OrderByDescending(vehicle => vehicle.Year)
                        .FirstOrDefault();

                    return new FeaturedVehicleViewModel
                    {
                        // Från familjen
                        FamilyId = family.FamilyId,
                        Name = family.Name,
                        Description = family.Description,
                        ImagePath = family.ImagePath ?? car?.ImagePath,
                        ComfortType = family.ComfortType,
                        CategoryType = family.CategoryType,
                        Transmission = family.TransmissionType,

                        // Från bilen
                        ModelName = car == null ? null : $"{car.Model.Brand.BrandName} {car.Model.ModelName}",
                        Year = car?.Year,
                        FuelType = car?.Engine.FuelType,
                        HorsePower = car?.Engine.HorsePower,
                        LuggageLitres = car?.LuggageCapacity,

                        // Från CarRateCatalog. Samma pris som GetDailyPrice(car),
                        // eftersom Matches kräver samma komfort och kaross.
                        BaseRate = _carRates.GetBaseDayRate(family.ComfortType),
                        BodyTypeFactor = _carRates.GetBodyTypeRate(family.CategoryType)
                    };
                })
                .ToList();
        }
        // Personbilskategorier med "from"-pris och skåpbilsfamiljer med pris och tillgänglighet.
        // Används av VehicleCategories-sektionen på startsidan.
        public VehicleCategoriesViewModel GetVehicleCategories()
        {
            var families = _familyRepository.GetAll().ToList();
            var carFamilies = families.OfType<PassengerCarFamily>().ToList();

            // Personbilar: en rad per kategori, billigaste familjen ger "from"-priset
            var cars = CarCategoryOrder
                .Select(category =>
                {
                    var prices = carFamilies
                        .Where(family => family.CategoryType == category)
                        .Select(family =>
                            _carRates.GetBaseDayRate(family.ComfortType) *
                            _carRates.GetBodyTypeRate(family.CategoryType))
                        .ToList();

                    return new CarCategoryViewModel
                    {
                        Category = category,
                        FromPrice = prices.Count > 0 ? prices.Min() : null   // null = ingen familj i kategorin än
                    };
                })
                .ToList();

            // Skåpbilar: en rad per seedad VanFamily
            var vans = families
                .OfType<VanFamily>()
                .OrderBy(family => Array.IndexOf(VanCategoryOrder, family.CategoryType))
                .Select(family => new VanCategoryViewModel
                {
                    FamilyId = family.FamilyId,
                    Category = family.CategoryType,
                    Name = family.Name,
                    Description = family.Description,
                    Transmission = family.TransmissionType,
                    MinPassengerCapacity = family.MinPassengerCapacity,
                    MinCargoVolumeM3 = family.MinCargoVolumeM3,
                    MinPayloadKg = family.MinPayloadKg,
                    DailyPrice = _vanRates.GetVanTypeRate(family.CategoryType),

                    // Tillgänglig när minst en körbar skåpbil är kopplad till familjen
                    IsAvailable = _assignmentService
                        .FindMatchingVehicles(family.FamilyId)
                        .Any(vehicle => vehicle.IsOperational)
                })
                .ToList();

            return new VehicleCategoriesViewModel
            {
                Cars = cars,
                Vans = vans
            };
        }

        // Sex rutor till "Vehicle categories" på startsidan:
        // en per personbilskategori (billigaste familjen) och en per skåpbilsfamilj.
        public List<VehicleCategoryTileViewModel> GetVehicleCategoryTiles()
        {
            var families = _familyRepository.GetAll().ToList();
            var carFamilies = families.OfType<PassengerCarFamily>().ToList();

            // ---- Personbilar: billigaste familjen i kategorin ger pris, bild och länk ----
            var carTiles = CarCategoryOrder
                .Select(category =>
                {
                    var cheapest = carFamilies
                        .Where(family => family.CategoryType == category)
                        .Select(family => new
                        {
                            Family = family,
                            Price = _carRates.GetBaseDayRate(family.ComfortType) *
                                    _carRates.GetBodyTypeRate(family.CategoryType)
                        })
                        .OrderBy(x => x.Price)
                        .FirstOrDefault();

                    return new VehicleCategoryTileViewModel
                    {
                        IsVan = false,
                        Label = CarCategoryLabel(category),
                        Description = CarCategoryDescription(category),
                        ImagePath = cheapest == null ? null : ImageFor(cheapest.Family),
                        FamilyId = cheapest?.Family.FamilyId,
                        Price = cheapest?.Price,
                        IsAvailable = cheapest != null
                    };
                });

            // ---- Skåpbilar: en ruta per seedad VanFamily ----
            var vanTiles = families
                .OfType<VanFamily>()
                .OrderBy(family => Array.IndexOf(VanCategoryOrder, family.CategoryType))
                .Select(family => new VehicleCategoryTileViewModel
                {
                    IsVan = true,
                    Label = family.CategoryType == VanCategoryType.PassengerVan ? "Passenger van" : "Cargo van",
                    Description = VanSpecs(family),
                    ImagePath = ImageFor(family),
                    FamilyId = family.FamilyId,
                    Price = _vanRates.GetVanTypeRate(family.CategoryType),

                    // Tillgänglig när minst en körbar skåpbil är kopplad till familjen
                    IsAvailable = _assignmentService
                        .FindMatchingVehicles(family.FamilyId)
                        .Any(vehicle => vehicle.IsOperational)
                });

            return carTiles.Concat(vanTiles).ToList();
        }

        // ---- Hjälpmetoder för rutorna ----

        // Familjens bild om den finns, annars bilden på ett fordon i familjen
        private string? ImageFor(VehicleFamily family)
        {
            return family.ImagePath
                ?? _assignmentService.FindMatchingVehicles(family.FamilyId)
                    .Select(vehicle => vehicle.ImagePath)
                    .FirstOrDefault(path => !string.IsNullOrEmpty(path));
        }

        private static string CarCategoryLabel(PassengerCarCategoryType category) => category switch
        {
            PassengerCarCategoryType.StationWagon => "Station wagon",
            _ => category.ToString()
        };

        private static string CarCategoryDescription(PassengerCarCategoryType category) => category switch
        {
            PassengerCarCategoryType.Hatchback => "Compact and easy to park",
            PassengerCarCategoryType.Sedan => "Comfort on longer drives",
            PassengerCarCategoryType.StationWagon => "Most luggage space",
            PassengerCarCategoryType.SUV => "High seating, room for all",
            _ => string.Empty
        };

        // T.ex. "7+ seats · Automatic" eller "8+ m³ · 500+ kg · Manual"
        private static string VanSpecs(VanFamily family)
        {
            var parts = new List<string>();
            if (family.MinPassengerCapacity is int seats) parts.Add($"{seats}+ seats");
            if (family.MinCargoVolumeM3 is double m3) parts.Add($"{m3:0}+ m³");
            if (family.MinPayloadKg is double kg) parts.Add($"{kg:0}+ kg");
            parts.Add(family.TransmissionType.ToString());
            return string.Join(" · ", parts);
        }
    }
}