using System;

public class PriceCatalog
{
    private readonly Dictionary<PriceClass, decimal> _baseDayRates = new()
    {
        [PriceClass.Economy] = 450m,
        [PriceClass.Standard] = 650m,
        [PriceClass.Premium] = 800m,
        [PriceClass.Luxury] = 1200m,
    };

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

    public decimal GetBaseDayRate(PriceClass category)
    {
        if (_baseDayRates.TryGetValue(category, out decimal categoryHourlyRate))
        {
            return categoryHourlyRate;
        }
        throw new KeyNotFoundException("Invalid category");
    }
}


/* Om man vill använda strängar istället för enum, så att man kan lägga till nya kategorier dynamiskt:

public class PriceCatalog 
{
    private readonly Dictionary<string, decimal> _priceClasses = new()
    {
        { "Economy" = 55m },
        { "Standard" = 70m},
        { "Premium" = 95m },
        { "Van" = 110m },
        { "LargeVan" = 130m }
    };

    public void SetBaseHourRate(string category, decimal newPrice)
    {
        if (!_priceClasses.ContainsKey(category))
        {
            throw new KeyNotFoundException("Invalid category");
        }
        if (newPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newPrice), "Price cannot be negative");
        }
        _priceClasses[category] = newPrice;
    }

    public decimal GetBaseHourRate(string category)
    {
        if (_priceClasses.TryGetValue(category, out decimal categoryHourlyRate))
        {
            return categoryHourlyRate;
        }
        throw new KeyNotFoundException("Invalid category");
    }

    public void addPriceClass(string category, decimal price)
    {
        if (_priceClasses.ContainsKey(category))
        {
            throw new ArgumentException("Category already exists");
        }
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");
        }
        _priceClasses[category] = price;
    }

    public void removePriceClass(string category)
    {
        if (!_priceClasses.ContainsKey(category))
        {
            throw new KeyNotFoundException("Invalid category");
        }
        _priceClasses.Remove(category);
    }
}*/
