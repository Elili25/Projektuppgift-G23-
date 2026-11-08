using System;

public class StandardBasePriceStrategy : IBasePriceStrategy
{
    public decimal Fee { get; } = 0;
    public decimal priceMultiplier { get; } = 1.0m;

    public decimal CalculateTotalPrice(decimal baseCost)
    {
        return baseCost * priceMultiplier;
    }
}
