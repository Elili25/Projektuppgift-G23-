using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.ViewModels
{
    public class VehicleCategoriesViewModel
    {
        public List<CarCategoryViewModel> Cars { get; set; } = new();
        public List<VanCategoryViewModel> Vans { get; set; } = new();

        // "Coming soon" visas tills minst en skåpbil finns i flottan
        public bool VansComingSoon => !Vans.Any(v => v.IsAvailable);
    }

    public class CarCategoryViewModel
    {
        public PassengerCarCategoryType Category { get; set; }
        public decimal? FromPrice { get; set; }          // null = ingen familj i kategorin än

        public string Label => Category switch
        {
            PassengerCarCategoryType.StationWagon => "Station wagon",
            _ => Category.ToString()
        };
    }

    public class VanCategoryViewModel
    {
        public Guid FamilyId { get; set; }
        public VanCategoryType Category { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TransmissionType Transmission { get; set; }
        public int? MinPassengerCapacity { get; set; }
        public double? MinCargoVolumeM3 { get; set; }
        public double? MinPayloadKg { get; set; }
        public decimal DailyPrice { get; set; }
        public bool IsAvailable { get; set; }            // finns det skåpbilar i familjen?
    }
}