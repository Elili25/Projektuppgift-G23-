using System;

public interface IBasePriceStrategy
{
    public decimal baseFee { get; }
    public decimal priceMultiplier { get; }
    public decimal CalculateBasePrice(decimal billableHours, decimal HourlyRate);
}
