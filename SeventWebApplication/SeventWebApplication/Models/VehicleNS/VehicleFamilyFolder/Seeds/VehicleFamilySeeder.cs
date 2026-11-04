namespace SeventWebApplication.Models.VehicleNS
{
    public static class VehicleFamilySeeder
    {
        public static List<VehicleFamily> GetSeededFamilies()
        {
            return new List<VehicleFamily>
            {
                // Passenger Cars
                new PassengerCarFamily(
                    Guid.NewGuid(),
                    "Comfort Sedan",
                    "Comfortable sedan for everyday driving.",
                    TransmissionType.Automatic,
                    VehicleComfortType.Luxury,
                    PassengerCarCategoryType.Sedan
                ),

                new PassengerCarFamily(
                    Guid.NewGuid(),
                    "Premium SUV",
                    "Premium SUV with high comfort.",
                    TransmissionType.Automatic,
                    VehicleComfortType.Premium,
                    PassengerCarCategoryType.SUV
                ),

                new PassengerCarFamily(
                    Guid.NewGuid(),
                    "Standard Hatchback",
                    "Practical hatchback for city driving.",
                    TransmissionType.Manual,
                    VehicleComfortType.Standard,
                    PassengerCarCategoryType.Hatchback
                ),

                // Vans
                new VanFamily(
                    Guid.NewGuid(),
                    "Cargo Van",
                    "Van for transporting larger goods.",
                    TransmissionType.Manual,
                    VanCategoryType.CargoVan,
                    8,
                    500,
                    null
                ),

                new VanFamily(
                    Guid.NewGuid(),
                    "Passenger Van",
                    "Van for transporting larger groups.",
                    TransmissionType.Automatic,
                    VanCategoryType.PassengerVan,
                    null,
                    null,
                    7
                )
            };
        }
    }
}