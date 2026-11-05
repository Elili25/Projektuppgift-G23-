using SeventWebApplication.Models.Addon.Interfaces;

namespace SeventWebApplication.Models.Addon
{
    public class ExtraDriverAddon : IAddon
    {
        public string DisplayName => "Extra förare";

        public int MaxLimit => 1;

        public int Quantity { get; set; } = 1;  
        private readonly decimal _price = 300;  

        public decimal Price => _price; 

        public decimal GetPrice() => _price * Quantity;
    }

}
