

using System;
using SeventWebApplication.Models.Pricing.PriceStrategy;

namespace SeventWebApplication.Models.Pricing.PriceStrategy.PriceStrategyFactories
{
public static class PriceStrategyFactory 
{
    public static IBasePriceStrategy ChoiceOfPriceStrategy(PriceStrategies choiceOfStrategy) => choiceOfStrategy switch
    {
       PriceStrategies.standard => new StandardBasePriceStrategy(),
       PriceStrategies.employeeDiscount => new EmployeeDiscountPriceStrategy(),
        _ => new StandardBasePriceStrategy()
    };
}
}
