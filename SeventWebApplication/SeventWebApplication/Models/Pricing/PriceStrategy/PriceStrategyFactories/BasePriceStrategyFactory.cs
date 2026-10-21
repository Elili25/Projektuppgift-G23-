using System;

public abstract class BasePriceStrategyFactory 
{
   protected abstract IPricingStrategy CreatePriceStrategy();
}

public class basePriceStrategyFactory //Ett simple-factory om vi vill ha en mindre komplex lösning 
{
    public IPricingStrategy ChoiceOfPriceStrategy(string choiceOfStrategy) => choiceOfStrategy.ToLower() switch
    {
        "standard" => new StandardPricingStrategy(),
        _ => new StandardPricingStrategy()

    };
}
