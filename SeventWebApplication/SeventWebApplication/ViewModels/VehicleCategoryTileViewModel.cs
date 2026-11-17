namespace SeventWebApplication.ViewModels
{
    // En ruta i "Vehicle categories"-sektionen på startsidan.
    // Används för både personbilskategorier och skåpbilsfamiljer.
    public class VehicleCategoryTileViewModel
    {
        public bool IsVan { get; set; }                       // styr mörk ruta och "Van"-etikett
        public string Label { get; set; } = string.Empty;     // "Hatchback", "Cargo van"
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

        public Guid? FamilyId { get; set; }                   // familjen som rutan länkar till
        public decimal? Price { get; set; }                   // null = inget pris än
        public bool IsAvailable { get; set; }                 // false = "Coming soon"

        public string Tag => IsVan ? "Van" : "Car";
    }
}