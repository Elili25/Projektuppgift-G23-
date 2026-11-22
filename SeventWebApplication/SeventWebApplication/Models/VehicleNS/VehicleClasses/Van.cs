using SeventWebApplication.Models.VehicleNS.Engine;

namespace SeventWebApplication.Models.VehicleNS
{
    // En skåpbil. Kategori (CargoVan/PassengerVan), lastvolym och maxlast avgör
    // vilken fordonsfamilj bilen hamnar i (VanFamily.Matches) och dagspriset (VanRateCatalog).
    public class Van : Vehicle,
        IPassengerVehicle,
        IVehicleTransmissionType,
        ICargoVehicle
    {
        // private set: om lastvärdena ändras i efterhand stämmer bilens familjekoppling inte längre
        public double CargoCapacityM3 { get; private set; }
        public double MaxPayloadKg { get; private set; }

        public VanCategoryType VanCategoryType { get; private set; }

        public Van(
            RegistrationPlate registrationPlate,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            VanCategoryType vanCategoryType,
            int year,
            int passengerCapacity,
            decimal luggageCapacity,
            IEngine engine,
            TransmissionType transmissionType,
            double cargoCapacityM3,
            double maxPayloadKg)
            : base(
                registrationPlate,
                model,
                vehicleCategoryType,
                year,
                passengerCapacity,
                luggageCapacity,
                engine,
                transmissionType)
        {
            if (vehicleCategoryType != VehicleCategoryType.Van)
                throw new ArgumentException("A van must have category Van.", nameof(vehicleCategoryType));

            if (passengerCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(passengerCapacity), "Passenger capacity must be greater than 0.");

            if (luggageCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(luggageCapacity), "Luggage capacity cannot be negative.");

            if (cargoCapacityM3 <= 0)
                throw new ArgumentOutOfRangeException(nameof(cargoCapacityM3), "Cargo capacity must be greater than 0.");

            if (maxPayloadKg <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPayloadKg), "Max payload must be greater than 0.");

            VanCategoryType = vanCategoryType;
            CargoCapacityM3 = cargoCapacityM3;
            MaxPayloadKg = maxPayloadKg;
        }
    }
}