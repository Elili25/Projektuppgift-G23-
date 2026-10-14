namespace SeventWebApplication.Models.Addon
{
    public class ExtraDriverAddon : AddonDecorator
    {
        private readonly decimal _price = 300;

        public ExtraDriverAddon(IAddon addon) : base(addon) { }

        public override decimal GetPrice()
        {
            return base.GetPrice() + _price;
        }
    }
}
