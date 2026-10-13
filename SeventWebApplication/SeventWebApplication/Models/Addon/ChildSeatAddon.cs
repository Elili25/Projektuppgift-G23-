namespace SeventWebApplication.Models.Addon
{
    public class ChildSeatAddon : AddonDecorator
    {
        private readonly decimal _price = 100;

        public ChildSeatAddon(IAddon addon) : base(addon) { }

        public override decimal GetPrice()
        {
            return base.GetPrice() + _price;
        }
    }
}
