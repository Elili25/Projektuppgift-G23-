using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Enums;
using SeventWebApplication.Models.Addon.Interfaces;

namespace SeventWebApplication.Models.Addon
{
    public class ChildSeatAddon : IAddon
    {
        public string DisplayName => "Barnstolar";
        public int MaxLimit => 3;

        public AddonType AddonType => AddonType.ChildSeat;

        public int Quantity { get; set; } = 1;  // Bas kvantitet är 1 men kan ändras när ett objekt läggs till
        private readonly decimal _price = 100;  // Baspriset på en addon

        public decimal Price => _price;  // Om man vill läsa baspriset utifrån

        public decimal GetPrice() => _price * Quantity;
    }

}