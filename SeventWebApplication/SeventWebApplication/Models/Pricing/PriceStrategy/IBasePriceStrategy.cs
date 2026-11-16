using System;

public interface IBasePriceStrategy
{
    public decimal Fee { get; }
    public decimal priceMultiplier { get; }
    public decimal CalculateTotalPrice(decimal BaseCost);
}
