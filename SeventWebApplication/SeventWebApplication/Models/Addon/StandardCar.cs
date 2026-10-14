namespace SeventWebApplication.Models.Addon
{
    public class StandardCar : IAddon
    {
        private readonly decimal _standardPrice;

        public StandardCar(decimal standardPrice)
        {
            _standardPrice = standardPrice;
        }

        public decimal GetPrice()
        {
            return _standardPrice;
        }
    }
}
