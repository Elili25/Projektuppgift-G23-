using System;

public class CustomBasePriceStrategy : IBasePriceStrategy
{
    public decimal baseFee { get; }
    public decimal priceMultiplier { get; }

    CustomBasePriceStrategy(decimal customBaseFee, decimal customPriceMultiplier)
    {
        baseFee = customBaseFee;
        priceMultiplier = customPriceMultiplier;
    }

    public decimal CalculateBasePrice(decimal billableHours, decimal hourlyRate)
    {
        return (baseFee + (billableHours * hourlyRate)) * priceMultiplier;
    }
}
