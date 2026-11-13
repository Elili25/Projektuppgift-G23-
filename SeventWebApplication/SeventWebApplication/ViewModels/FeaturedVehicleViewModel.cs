namespace SeventWebApplication.ViewModels
{
    public class FeaturedVehicleViewModel
    {
        public Guid FamilyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImagePath { get; set; }
    }
}