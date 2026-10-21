using System;

public class BodyCatalog
{
    private readonly Dictionary<CarBodyType, decimal> _carBodyTypeRates = new()
    {
        [CarBodyType.Compact] = 0.9m,
        [CarBodyType.Sedan] = 1m,
        [CarBodyType.Estate] = 1.05m,
        [CarBodyType.SUV] = 1.10m,
        [CarBodyType.Van] = 2m
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

    public decimal GetBodyTypeRate(CarBodyType category)
    {
        if (_carBodyTypeRates.TryGetValue(category, out decimal categoryRate))
        {
            return categoryRate;
        }
        throw new KeyNotFoundException("Invalid category");
    }
}


/*Om man vill använda strängar istället för enum, så att man kan lägga till nya kategorier dynamiskt:
 
public class BodyCatalog
{
    private readonly Dictionary<string, decimal> _carBodyTypeRates = new()
    {
        { "Compact" = 0.9m },
        { "Sedan" = 1m },
        { "Estate" = 1.05m },
        { "SUV" = 1.10m }
    };

    public void SetBodyTypeRate(string category, decimal newRate)
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

    public decimal GetBodyTypeRate(string category)
    {
        if (_carBodyTypeRates.TryGetValue(category, out decimal categoryHourlyRate))
        {
            return categoryHourlyRate;
        }
        throw new KeyNotFoundException("Invalid category");
    }
}

public void addBodyType(string category, decimal price)
{
    if (_carBodyTypeRates.ContainsKey(category))
    {
        throw new ArgumentException("Category already exists");
    }
    if (price < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");
    }
    _carBodyTypeRates[category] = price;
}

public void removePriceClass(string category)
{
    if (!_carBodyTypeRates.ContainsKey(category))
    {
        throw new KeyNotFoundException("Invalid category");
    }
    _carBodyTypeRates.Remove(category);
}
}*/