using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.ViewModels
{
    public class FeaturedVehicleViewModel
    {
        // Från familjen
        public Guid FamilyId { get; set; }
        public string Name { get; set; } = string.Empty;          // "Economy Station Wagon"
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public VehicleComfortType ComfortType { get; set; }
        public PassengerCarCategoryType CategoryType { get; set; }
        public TransmissionType Transmission { get; set; }

        // Från en matchande bil (null om familjen saknar bilar)
        public string? ModelName { get; set; }                     // "Volvo V60"
        public int? Year { get; set; }
        public FuelType? FuelType { get; set; }
        public int? HorsePower { get; set; }
        public decimal? LuggageLitres { get; set; }

        // Från CarRateCatalog
        public decimal BaseRate { get; set; }
        public decimal BodyTypeFactor { get; set; }
        public decimal DailyPrice => BaseRate * BodyTypeFactor;

        // Hjälp för vyn
        public string Tier => ComfortType.ToString().ToLowerInvariant();   // "economy" -> id/klasser i CSS:en
        public string CategoryLabel => CategoryType switch
        {
            PassengerCarCategoryType.StationWagon => "Station wagon",
            PassengerCarCategoryType.SUV => "SUV",
            _ => CategoryType.ToString()
        };
    }
}