namespace SeventWebApplication.Models.Addon
{
    public abstract class AddonDecorator : IAddon
    {
        protected readonly IAddon _wrappedAddon;

        protected AddonDecorator(IAddon addon)
        {
            _wrappedAddon = addon;
        }

        public IAddon GetWrappedAddon()
        {
            return _wrappedAddon;
        }

        public virtual decimal GetPrice()
        {
            return _wrappedAddon.GetPrice();
        }
    }

}
