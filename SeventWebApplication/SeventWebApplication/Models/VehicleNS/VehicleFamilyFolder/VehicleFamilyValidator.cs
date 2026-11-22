namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleFamilyValidator
    {
        public void ValidatePassengerCar(
            string name,
            TransmissionType transmissionType,
            VehicleComfortType comfortType,
            PassengerCarCategoryType categoryType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Family name is required.");

            if (!Enum.IsDefined(transmissionType))
                throw new ArgumentException("Invalid transmission type.");

            if (!Enum.IsDefined(comfortType))
                throw new ArgumentException("Invalid comfort type.");

            if (!Enum.IsDefined(categoryType))
                throw new ArgumentException("Invalid category.");
        }

        public void ValidateVan(
            string name,
            TransmissionType transmissionType,
            VanCategoryType categoryType,
            double? minCargoVolumeM3,
            double? minPayloadKg,
            int? minPassengerCapacity)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Family name is required.");

            if (!Enum.IsDefined(transmissionType))
                throw new ArgumentException("Invalid transmission type.");

            if (!Enum.IsDefined(categoryType))
                throw new ArgumentException("Invalid van category.");

            if (minCargoVolumeM3.HasValue && minCargoVolumeM3 <= 0)
                throw new ArgumentException("Cargo volume must be positive.");

            if (minPayloadKg.HasValue && minPayloadKg <= 0)
                throw new ArgumentException("Payload must be positive.");

            if (minPassengerCapacity.HasValue && minPassengerCapacity <= 0)
                throw new ArgumentException("Passenger capacity must be positive.");
        }
    }
}