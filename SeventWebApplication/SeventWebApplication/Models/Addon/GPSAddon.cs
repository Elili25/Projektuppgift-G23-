namespace SeventWebApplication.Models.Addon
{
    public class GPSAddon : AddonDecorator
    {
        public GPSAddon(IAddon addon) : base(addon) { }

        private readonly decimal _price = 500;

        //skapa public get för rabatter
        public decimal Price => _price;

        public override decimal GetPrice()
        {
            return base.GetPrice() + _price;
        }
    }

}
