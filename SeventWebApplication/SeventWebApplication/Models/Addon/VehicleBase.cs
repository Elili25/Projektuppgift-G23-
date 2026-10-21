namespace SeventWebApplication.Models.Addon
{
    public class VehicleBase : IAddon
    {
        private readonly decimal _standardPrice;

        public VehicleBase(decimal standardPrice)
        {
            _standardPrice = standardPrice;
        }

        public decimal GetPrice()
        {
            return _standardPrice;
        }
    }
}
