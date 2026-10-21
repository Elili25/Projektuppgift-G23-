using System;

public class StandardBasePriceStrategyFactory : BasePriceStrategyFactory
{
    protected override IBasePriceStrategy ChoiceOfStrategy() => new StandardBasePriceStrategy();
}
