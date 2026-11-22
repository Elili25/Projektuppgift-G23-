namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleFamilyFactory
    {
        public PassengerCarFamily CreatePassengerCarFamily(
            string name,
            string description,
            TransmissionType transmissionType,
            VehicleComfortType comfortType,
            PassengerCarCategoryType categoryType)
        {
            return new PassengerCarFamily(
                Guid.NewGuid(),
                name,
                description,
                transmissionType,
                comfortType,
                categoryType
            );
        }

        public VanFamily CreateVanFamily(
            string name,
            string description,
            TransmissionType transmissionType,
            VanCategoryType categoryType,
            double? minCargoVolumeM3,
            double? minPayloadKg,
            int? minPassengerCapacity)
        {
            return new VanFamily(
                Guid.NewGuid(),
                name,
                description,
                transmissionType,
                categoryType,
                minCargoVolumeM3,
                minPayloadKg,
                minPassengerCapacity
            );
        }
    }
}