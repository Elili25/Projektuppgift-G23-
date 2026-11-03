using SeventWebApplication.Models.Addon.Enums;

namespace SeventWebApplication.Models.Addon.Interfaces
{
    public interface IAddon
    {
        // Alla addons måste berätta vilken enum de är
        AddonType AddonType { get; }

        // Alla addons har ett antal som går att både läsa och ändra
        int Quantity { get; set; }

        // Alla addons kan räkna ut sitt totala pris
        decimal GetPrice();
    }
}
