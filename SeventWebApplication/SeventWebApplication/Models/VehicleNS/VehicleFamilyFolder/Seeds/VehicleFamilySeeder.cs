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

                // Matchar: Tesla Model 3, BMW i4
                new PassengerCarFamily(
                    LuxurySedanId,
                    "Luxury Sedan",
                    "Luxurious sedan with automatic transmission.",
                    TransmissionType.Automatic,
                    VehicleComfortType.Luxury,
                    PassengerCarCategoryType.Sedan),

                // Matchar: Volvo XC60
                new PassengerCarFamily(
                    PremiumSuvId,
                    "Premium SUV",
                    "Premium SUV with high comfort.",
                    TransmissionType.Automatic,
                    VehicleComfortType.Premium,
                    PassengerCarCategoryType.SUV),

                // Matchar: Volkswagen Golf
                new PassengerCarFamily(
                    StandardHatchbackId,
                    "Standard Hatchback",
                    "Practical hatchback for city driving.",
                    TransmissionType.Manual,
                    VehicleComfortType.Standard,
                    PassengerCarCategoryType.Hatchback),

                // Matchar: Volvo V60 från FleetManager.SeedTestVehicle
                new PassengerCarFamily(
                    EconomyWagonId,
                    "Economy Station Wagon",
                    "Affordable station wagon with plenty of luggage space.",
                    TransmissionType.Automatic,
                    VehicleComfortType.Economy,
                    PassengerCarCategoryType.StationWagon),

                // ---- Skåpbilar ----
                // Inga skåpbilar seedas än, så de här är tomma tills en Van läggs till i flottan.

                new VanFamily(
                    CargoVanId,
                    "Cargo Van",
                    "Van for transporting larger goods.",
                    TransmissionType.Manual,
                    VanCategoryType.CargoVan,
                    minCargoVolumeM3: 8,
                    minPayloadKg: 500),

                new VanFamily(
                    PassengerVanId,
                    "Passenger Van",
                    "Van for transporting larger groups.",
                    TransmissionType.Automatic,
                    VanCategoryType.PassengerVan,
                    minPassengerCapacity: 7)
            };
        }
    }
}