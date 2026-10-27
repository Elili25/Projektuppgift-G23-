namespace SeventWebApplication.Models.VehicleNS
{
    public class VanFamily : VehicleFamily, IVanFamily
    {
        public VanCategoryType CategoryType { get; private set; }
        public double? MinCargoVolumeM3 { get; private set; }
        public double? MinPayloadKg { get; private set; }
        public int? MinPassengerCapacity { get; private set; }

        public VanFamily(
            Guid familyId,
            string name,
            string description,
            TransmissionType transmissionType,
            VanCategoryType categoryType,
            double? minCargoVolumeM3 = null,
            double? minPayloadKg = null,
            int? minPassengerCapacity = null)
            : base(
                familyId,
                name,
                description,
                VehicleCategoryType.Van,
                transmissionType)
        {
            CategoryType = categoryType;
            MinCargoVolumeM3 = minCargoVolumeM3;
            MinPayloadKg = minPayloadKg;
            MinPassengerCapacity = minPassengerCapacity;
        }

        public override bool Matches(Vehicle vehicle)
        {
            if (vehicle is not Van van)
                return false;

            if (van.VanCategoryType != CategoryType)
                return false;

            if (van.TransmissionType != TransmissionType)
                return false;

            if (MinCargoVolumeM3.HasValue &&
                van.CargoCapacityM3 < MinCargoVolumeM3.Value)
                return false;

            if (MinPayloadKg.HasValue &&
                van.MaxPayloadKg < MinPayloadKg.Value)
                return false;

            if (MinPassengerCapacity.HasValue &&
                van.PassengerCapacity < MinPassengerCapacity.Value)
                return false;

            return true;
        }
    }
}

