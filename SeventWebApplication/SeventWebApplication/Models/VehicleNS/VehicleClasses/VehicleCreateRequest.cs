namespace SeventWebApplication.Models.VehicleNS
{
    // Gemensamt för alla fordon
    public abstract record VehicleCreateRequest
    {
        public required string RegNr { get; init; }
        public required CountryCode CountryCode { get; init; }
        public required VehicleModel Model { get; init; }
        public required int Year { get; init; }
        public required int HorsePower { get; init; }
        public required int PassengerCapacity { get; init; }
        public required decimal LuggageCapacity { get; init; }
        public required FuelType FuelType { get; init; }
        public required TransmissionType TransmissionType { get; init; }

        // Förbränningsmotor
        public int FuelCapacityLiters { get; init; } = 50;

        // Elmotor (används bara när FuelType = Electric)
        public int BatteryCapacityKwh { get; init; }
        public int RangeKm { get; init; }
    }

    public record CarCreateRequest : VehicleCreateRequest
    {
        public required PassengerCarCategoryType CategoryType { get; init; }
        public required VehicleComfortType ComfortType { get; init; }
    }

    public record VanCreateRequest : VehicleCreateRequest
    {
        public required VanCategoryType CategoryType { get; init; }
        public required double CargoCapacityM3 { get; init; }
        public required double MaxPayloadKg { get; init; }
    }
}
