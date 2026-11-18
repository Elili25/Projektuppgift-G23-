namespace SeventWebApplication.Models.Guest
{
    // En grupp specifikationer på produktsidan (t.ex. "Capacity"). Sidan ritar ut vilka grupper och rader som helst,
    // så nya specifikationer läggs bara till i VehiclesController.BuildFamilyPage utan att vyn behöver ändras.
    public class SpecGroup
    {
        public string Title { get; init; } = "";
        public List<(string Label, string Value)> Items { get; init; } = new();
    }

    // En modell i familjen, t.ex. "Volvo V60 · 2024"
    public class FamilyModelItem
    {
        public string Name { get; init; } = "";
        public string Details { get; init; } = "";   // t.ex. "197 hp · Petrol · 5 seats"
    }

    // Produktsidan för en fordonsfamilj (/Vehicles/Family/{id})
    public class FamilyPageViewModel
    {
        public Guid FamilyId { get; init; }
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public bool IsVan { get; init; }
        public string Category { get; init; } = "";
        public string? ImagePath { get; init; }
        public decimal? FromDailyPrice { get; init; }

        public List<string> Tags { get; init; } = new();                         // små brickor under rubriken
        public List<(string Label, string Value)> Highlights { get; init; } = new();   // nyckelfakta i en rad
        public List<SpecGroup> SpecGroups { get; init; } = new();
        public List<FamilyModelItem> Models { get; init; } = new();
    }
}
