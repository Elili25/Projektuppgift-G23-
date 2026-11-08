using System;

public class EmployeeDiscountPriceStrategy : IBasePriceStrategy
{
    public decimal Fee { get; } = 0;
    public decimal priceMultiplier { get; } = 0.85m;
    

    public decimal CalculateTotalPrice(decimal baseCost)
    {
        return baseCost * priceMultiplier;
    }
}
