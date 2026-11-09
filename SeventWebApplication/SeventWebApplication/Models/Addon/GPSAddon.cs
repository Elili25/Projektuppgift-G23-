using SeventWebApplication.Models.Addon.Enums;
using SeventWebApplication.Models.Addon.Interfaces;

namespace SeventWebApplication.Models.Addon
{
    public class GPSAddon : IAddon
    {
        public string DisplayName => "GPS";
        public int MaxLimit => 1;

        public AddonType AddonType => AddonType.Gps;

        public int Quantity { get; set; } = 1;
        private readonly decimal _price = 500;  

        public decimal Price => _price; 

        public decimal GetPrice() => _price * Quantity;
    }

}
