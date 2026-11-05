
namespace SeventWebApplication.Models.Addon.Interfaces
{
    public interface IAddon
    {
        // Det svenska namnet för en addon som visas i felmeddelande 
        string DisplayName { get; }
        
        // Maxantalet för en addon
        int MaxLimit { get; }

        // Alla addons har ett antal som går att både läsa och ändra
        int Quantity { get; set; }

        // Alla addons kan räkna ut sitt totala pris
        decimal GetPrice();
    }
}
