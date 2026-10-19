using System;

public class PriceCatalog
{
    private readonly Dictionary<PriceClass, decimal> _baseHourRates = new()
    {
        [PriceClass.Economy] = 55m,
        [PriceClass.Standard] = 70m,
        [PriceClass.Premium] = 95m,
        [PriceClass.Van] = 110m,
        [PriceClass.largeVan] = 130m
    };

    public void SetBaseHourRate(PriceClass category, decimal newPrice)
    {
        if (!_baseHourRates.ContainsKey(category))
        {
            throw new KeyNotFoundException("Invalid category");
        }

        if (newPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newPrice), "Price cannot be negative");
        }

        _baseHourRates[category] = newPrice;
    }

    public decimal GetBaseHourRate(PriceClass category)
    {
        if (_baseHourRates.TryGetValue(category, out decimal categoryHourlyRate))
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
}
