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

        // Enumen har ordningen Economy, Standard, Luxury, Premium, så panelerna sorteras explicit
        private static readonly VehicleComfortType[] TierOrder =
        {
            VehicleComfortType.Economy,
            VehicleComfortType.Standard,
            VehicleComfortType.Premium,
            VehicleComfortType.Luxury
        };

        public VehicleFamilyService(
            VehicleFamilyFactory familyFactory,
            VehicleFamilyRepository familyRepository,
            VehicleFamilyValidator validator,
            VehicleAssignmentService assignmentService,
            CarRateCatalog carRates)
        {
            _familyFactory = familyFactory;
            _familyRepository = familyRepository;
            _validator = validator;
            _assignmentService = assignmentService;
            _carRates = carRates;
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
    }
}