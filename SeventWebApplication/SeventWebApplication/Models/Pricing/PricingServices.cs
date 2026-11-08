using SeventWebApplication.Models.Pricing.PriceStrategy.PriceStrategyFactories;
using SeventWebApplication.Models.Pricing.PricingCategories;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Pricing;

public class PricingServices
{
    private readonly CarRateCatalog _carRateCatalog;
    private readonly VanRateCatalog _vanRateCatalog;
    private readonly BaseCosts _baseCosts;
    
    public PricingServices(CarRateCatalog carRateCatalog, VanRateCatalog vanRateCatalog,  BaseCosts baseCosts)
    {
        _vanRateCatalog = vanRateCatalog;
        _carRateCatalog = carRateCatalog;
        _baseCosts = baseCosts;
    }

    public decimal CalculateBaseRate(Vehicle vehicle, int billableDays)
    {
       return vehicle switch
        { 
           Car car => _carRateCatalog.GetDailyPrice(car) * billableDays,
           
           Van van => _vanRateCatalog.GetDailyPrice(van) *  billableDays,
           
           _=> throw new ArgumentOutOfRangeException(nameof(vehicle), vehicle, null)
        };
    }
    
    public decimal CalculateTotalPrice(PriceStrategies priceStrategy, Booking.Booking booking, Vehicle vehicle, int billableDays)
    {
        IBasePriceStrategy PriceStrategy = PriceStrategyFactory.ChoiceOfPriceStrategy(priceStrategy);
        
        return PriceStrategy.CalculateTotalPrice(CalculateBaseRate(vehicle, billableDays)) + _baseCosts.GetBaseFee() + booking.Addon.GetTotalAddonPrice();
    }
    
    public decimal CalculateLateFee(string bookingId, TimeSpan delay)
    {
        var hours = (decimal)Math.Ceiling(delay.TotalHours);
        return hours * _baseCosts.GetDepositFee();
    }

    public decimal CalculateDepositFee(Vehicle vehicle, int billableDays)
    {
        return _baseCosts.GetDepositFee() * (decimal)Math.Log10((double)CalculateBaseRate(vehicle, billableDays)); //dubbel cast lol
    }
}

    