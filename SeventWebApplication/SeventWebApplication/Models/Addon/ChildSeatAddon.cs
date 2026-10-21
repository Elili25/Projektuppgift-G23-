using SeventWebApplication.Models;
using SeventWebApplication.Models.Addon;

namespace SeventWebApplication.Models.Addon
{
    public class ChildSeatAddon : AddonDecorator
    {
        private readonly decimal _price = 100;

        public decimal Price => _price;
        //skapa public get för rabatter

        public ChildSeatAddon(IAddon addon) : base(addon) { }

        public override decimal GetPrice()
        {
            return base.GetPrice() + _price;
        }

    }
}