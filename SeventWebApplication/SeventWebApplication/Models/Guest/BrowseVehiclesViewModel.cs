namespace SeventWebApplication.Models.Guest
{
    // Ett kort på /Vehicles: en fordonsfamilj och några av bilarna i den
    public class FamilyCardViewModel
    {
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public string Kind { get; init; } = "";           // Passenger car / Van
        public string Category { get; init; } = "";       // SUV, Cargo van ...
        public string Transmission { get; init; } = "";
        public string? Comfort { get; init; }             // bara personbilar
        public int VehicleCount { get; init; }
        public List<string> Vehicles { get; init; } = new();
    }

    public class BrowseVehiclesViewModel
    {
        public string Title { get; init; } = "All vehicles";
        public List<FamilyCardViewModel> Families { get; init; } = new();
    }
}
