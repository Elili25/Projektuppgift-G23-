namespace SeventWebApplication.Models.Guest
{
    // Ett kort i karusellen på /Vehicles/Cars och /Vehicles/Vans: en fordonsfamilj.
    // Antal fordon visas medvetet inte, det är intern information.
    public class FamilyCardViewModel
    {
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public string Category { get; init; } = "";       // SUV, Cargo van ...
        public string? ImagePath { get; init; }
        public string? Example { get; init; }             // t.ex. "Volvo V60", visas som "or similar"
        public List<string> Tags { get; init; } = new();  // växellåda, komfort
        public List<(string Label, string Value)> Specs { get; init; } = new();
    }

    public class BrowseVehiclesViewModel
    {
        public bool IsVans { get; init; }
        public string Title => IsVans ? "Vans" : "Passenger cars";
        public string? Category { get; init; }            // vald kategori (nyckel), annars alla
        public List<FamilyCardViewModel> Families { get; init; } = new();
    }
}
