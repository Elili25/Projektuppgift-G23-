using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Services
{
    public class VehicleFamilyService
    {
        private readonly VehicleFamilyFactory _familyFactory;
        private readonly VehicleFamilyRepository _familyRepository;
        private readonly VehicleFamilyValidator _validator;

        public VehicleFamilyService(
            VehicleFamilyFactory familyFactory,
            VehicleFamilyRepository familyRepository,
            VehicleFamilyValidator validator)
        {
            _familyFactory = familyFactory;
            _familyRepository = familyRepository;
            _validator = validator;
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
    }
}