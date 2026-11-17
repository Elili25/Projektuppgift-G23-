namespace SeventWebApplication.Models.VehicleNS
{
    // Färdiga fordonsfamiljer som läggs in vid uppstart (Program.cs).
    // Varje seedad bil i ExampleBookingSeeder och FleetManager.SeedTestVehicle matchar en familj här,
    // annars blir familjen tom och kan inte bokas.
    //
    // Id:na är fasta (inte Guid.NewGuid()), så de är samma vid varje omstart.
    // Då kan man testa t.ex. /Families/Availability?familyId=... utan att leta upp ett nytt id varje gång.
    public static class VehicleFamilySeeder
    {
        public static readonly Guid LuxurySedanId = Guid.Parse("11111111-0000-0000-0000-000000000001");
        public static readonly Guid PremiumSuvId = Guid.Parse("11111111-0000-0000-0000-000000000002");
        public static readonly Guid StandardHatchbackId = Guid.Parse("11111111-0000-0000-0000-000000000003");
        public static readonly Guid EconomyWagonId = Guid.Parse("11111111-0000-0000-0000-000000000004");
        public static readonly Guid CargoVanId = Guid.Parse("22222222-0000-0000-0000-000000000001");
        public static readonly Guid PassengerVanId = Guid.Parse("22222222-0000-0000-0000-000000000002");

        public static List<VehicleFamily> GetSeededFamilies()
        {
            return new List<VehicleFamily>
            {
                // ---- Personbilar ----

                // Luxury Sedan
                new PassengerCarFamily(
                    LuxurySedanId,
                    "Luxury Sedan",
                    "Luxurious sedan with automatic transmission.",
                    TransmissionType.Automatic,
                    VehicleComfortType.Luxury,
                    PassengerCarCategoryType.Sedan)
                {
                    ImagePath = "/lib/Images/FeaturedVehicleImages/PassengerCar/VehicleFamily-Luxury-Sedan-FeaturedImage.png"
                },

                // Premium SUV
                new PassengerCarFamily(
                    PremiumSuvId,
                    "Premium SUV",
                    "Premium SUV with high comfort.",
                    TransmissionType.Automatic,
                    VehicleComfortType.Premium,
                    PassengerCarCategoryType.SUV)
                {
                    ImagePath = "/lib/Images/FeaturedVehicleImages/PassengerCar/VehicleFamily-Premium-SUV-FeaturedImage.png"
                },

                // Standard Hatchback
                new PassengerCarFamily(
                    StandardHatchbackId,
                    "Standard Hatchback",
                    "Practical hatchback for city driving.",
                    TransmissionType.Manual,
                    VehicleComfortType.Standard,
                    PassengerCarCategoryType.Hatchback)
                {
                    ImagePath = "/lib/Images/FeaturedVehicleImages/PassengerCar/VehicleFamily-Standard-Hatchback-FeaturedImage.png"
                },

                // Economy Station Wagon
                new PassengerCarFamily(
                    EconomyWagonId,
                    "Economy Station Wagon",
                    "Affordable station wagon with plenty of luggage space.",
                    TransmissionType.Automatic,
                    VehicleComfortType.Economy,
                    PassengerCarCategoryType.StationWagon)
                {
                    ImagePath = "/lib/Images/FeaturedVehicleImages/PassengerCar/VehicleFamily-Economy-StationWagon-FeaturedImage.png"
                },

                // ---- Skåpbilar ----

                // Cargo Van
                new VanFamily(
                    CargoVanId,
                    "Cargo Van",
                    "Van for transporting larger goods.",
                    TransmissionType.Manual,
                    VanCategoryType.CargoVan,
                    minCargoVolumeM3: 8,
                    minPayloadKg: 500)
                {
                    ImagePath = "/lib/Images/FeaturedVehicleImages/Van/VehicleFamily-Cargo-Van-FeaturedImage.png"
                },

                // Passenger Van
                new VanFamily(
                    PassengerVanId,
                    "Passenger Van",
                    "Van for transporting larger groups.",
                    TransmissionType.Automatic,
                    VanCategoryType.PassengerVan,
                    minPassengerCapacity: 7)
                {
                    ImagePath = "/lib/Images/FeaturedVehicleImages/Van/VehicleFamily-PassengerVan-FeaturedImage.png"
                }
            };
        }
    }
}
