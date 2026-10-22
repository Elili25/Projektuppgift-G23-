using System.Reflection.Metadata.Ecma335;

namespace SeventWebApplication.Models.Pricing.PricingCategories;

public class RateCatalog
{
    private readonly Dictionary<CarBodyType, decimal> _carBodyTypeRates = new()
    {
        [CarBodyType.Compact] = 0.9m,
        [CarBodyType.Sedan] = 1m,
        [CarBodyType.Estate] = 1.1m,
        [CarBodyType.SUV] = 1.20m,
    };
    private readonly Dictionary<PriceClass, decimal> _baseDayRates = new()
    {
        [PriceClass.Economy] = 450m,
        [PriceClass.Standard] = 650m,
        [PriceClass.Premium] = 800m,
        [PriceClass.Luxury] = 1200m,
    };
    public void SetBodyTypeRate(CarBodyType category, decimal newRate)
    {
        if (!_carBodyTypeRates.ContainsKey(category))
        {
            throw new KeyNotFoundException("Invalid category");
        }

        if (newRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newRate), "Price cannot be negative");
        }

        _carBodyTypeRates[category] = newRate;
    }
    public void SetBaseDayRate(PriceClass category, decimal newPrice)
    {
        if (!_baseDayRates.ContainsKey(category))
        {
            throw new KeyNotFoundException("Invalid category");
        }

        if (newPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newPrice), "Price cannot be negative");
        }

        _baseDayRates[category] = newPrice;
    }

    public decimal GetDailyPrice(PriceClass category, CarBodyType carBodyType)
    {
        return _baseDayRates[category] * _carBodyTypeRates[carBodyType];
    }
}