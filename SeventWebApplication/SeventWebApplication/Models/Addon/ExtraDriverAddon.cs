namespace SeventWebApplication.Models.Addon
{
    public class ExtraDriverAddon : AddonDecorator
    {
        private readonly decimal _price = 300;

        public decimal Price => _price;
        //skapa public get för rabatter

        public ExtraDriverAddon(IAddon addon) : base(addon) { }

        public override decimal GetPrice()
        {
            return base.GetPrice() + _price;
        }
    }

}
