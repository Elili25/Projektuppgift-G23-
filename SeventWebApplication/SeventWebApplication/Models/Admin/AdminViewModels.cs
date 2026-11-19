using System.ComponentModel.DataAnnotations;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Admin
{
    // Rad i fordonslistorna (All vehicles / Service & repairs)
    public class VehicleRowViewModel
    {
        public Guid VehicleId { get; init; }
        public string RegNr { get; init; } = "";
        public string Name { get; init; } = "";
        public string? ImagePath { get; init; }
        public string Type { get; init; } = "";
        public int Year { get; init; }
        public VehicleStatus Status { get; init; }
        public bool IsRented { get; init; }
        public int OpenDamages { get; init; }
    }

    // Formuläret för "Add vehicle" (bil eller skåpbil)
    public class VehicleForm
    {
        public VehicleType Type { get; set; } = VehicleType.Car;

        public IFormFile? Image { get; set; }   // valfri bild (PNG, JPG eller WebP)

        [Required] public string RegNr { get; set; } = "";
        public CountryCode CountryCode { get; set; } = CountryCode.SE;
        [Required] public string Brand { get; set; } = "";
        [Required] public string ModelName { get; set; } = "";
        public int Year { get; set; } = DateTime.Now.Year;
        public int HorsePower { get; set; } = 120;
        public int PassengerCapacity { get; set; } = 5;
        public decimal LuggageCapacity { get; set; } = 400;
        public FuelType FuelType { get; set; } = FuelType.Petrol;
        public TransmissionType TransmissionType { get; set; } = TransmissionType.Manual;

        public int FuelCapacityLiters { get; set; } = 50;      // bensin/diesel/hybrid
        public int BatteryCapacityKwh { get; set; }            // el
        public int RangeKm { get; set; }                       // el

        // Bara bil
        public PassengerCarCategoryType CarCategory { get; set; } = PassengerCarCategoryType.Sedan;
        public VehicleComfortType Comfort { get; set; } = VehicleComfortType.Standard;

        // Bara skåpbil
        public VanCategoryType VanCategory { get; set; } = VanCategoryType.CargoVan;
        public double CargoCapacityM3 { get; set; } = 6;
        public double MaxPayloadKg { get; set; } = 1000;
    }

    // Formuläret för att skapa en fordonsfamilj
    public class FamilyForm
    {
        public VehicleType Type { get; set; } = VehicleType.Car;
        public IFormFile? Image { get; set; }   // valfri bild (PNG, JPG eller WebP)
        [Required] public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public TransmissionType TransmissionType { get; set; } = TransmissionType.Automatic;

        // Bil
        public VehicleComfortType Comfort { get; set; } = VehicleComfortType.Standard;
        public PassengerCarCategoryType CarCategory { get; set; } = PassengerCarCategoryType.Sedan;

        // Skåpbil
        public VanCategoryType VanCategory { get; set; } = VanCategoryType.CargoVan;
        public double? MinCargoVolumeM3 { get; set; }
        public double? MinPayloadKg { get; set; }
        public int? MinPassengerCapacity { get; set; }
    }

    public class FamilyRowViewModel
    {
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public string? ImagePath { get; init; }
        public string Kind { get; init; } = "";
        public string Transmission { get; init; } = "";
        public int VehicleCount { get; init; }
    }

    public class FamilyPageViewModel
    {
        public List<FamilyRowViewModel> Families { get; init; } = new();
        public FamilyForm Form { get; init; } = new();
    }

    // Formuläret för "Register return"
    public class ReturnForm
    {
        [Required] public string BookingId { get; set; } = "";
        public int Mileage { get; set; }
        public decimal? FuelLevel { get; set; }
        public decimal? BatteryLevel { get; set; }
        public InspectionResult InspectionResult { get; set; } = InspectionResult.Approved;
        public string? Damages { get; set; }   // en skada per rad
        [Range(0, double.MaxValue, ErrorMessage = "Damage fee cannot be negative.")]
        public decimal DamageFee { get; set; }  // skadeavgift i kr, anges av personalen
        public string? Notes { get; set; }
    }

    public class ReturnPageViewModel
    {
        public List<BookingRowViewModel> ActiveBookings { get; init; } = new();
        public ReturnForm Form { get; init; } = new();
    }

    // Formulär för upphämtning (Confirmed → Active)
    public class PickupForm
    {
        [Required] public string BookingId { get; set; } = "";
        [Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")]
        public int Mileage { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Fuel level cannot be negative.")]
        public decimal FuelLevel { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Battery level cannot be negative.")]
        public decimal BatteryLevel { get; set; }
        public string? Notes { get; set; }
    }

    public class PickupPageViewModel
    {
        public BookingRowViewModel Booking { get; init; } = new();
        public PickupForm Form { get; init; } = new();
    }

    public class DamageRowViewModel
    {
        public Guid DamageId { get; init; }
        public string RegNr { get; init; } = "";
        public string Vehicle { get; init; } = "";
        public string Description { get; init; } = "";
        public DateTime ReportedAt { get; init; }
        public bool IsResolved { get; init; }
    }
}
