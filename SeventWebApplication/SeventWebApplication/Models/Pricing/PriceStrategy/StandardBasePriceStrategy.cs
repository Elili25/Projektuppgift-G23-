using System;

public class StandardBasePriceStrategy : IBasePriceStrategy
{
    public decimal baseFee { get; } = 100;
    public decimal priceMultiplier { get; } = 1.0m;

    public decimal CalculateBasePrice(decimal billableHours, decimal hourlyRate)
    {
        return ( baseFee + (billableHours * hourlyRate) ) * priceMultiplier;
    }
}
