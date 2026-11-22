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
    
    // Räknar ut hyrespriset för bokningen med standardpris och sparar det på bokningen (Booking.TotalPrice).
    // Bilen avgör dagspriset. Vid en familjebokning utan bil skickas en bil ur familjen in (samma dagspris).
   
    public void ApplyPrice(Booking.Booking booking, Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(booking);
        ArgumentNullException.ThrowIfNull(vehicle);
        
        int days = Math.Max(1, (int)Math.Ceiling((booking.EndDate - booking.StartDate).TotalDays));

        booking.TotalPrice = CalculateTotalPrice(PriceStrategies.standard, booking, vehicle, days);
    }

    public decimal CalculateLateFee(string bookingId, TimeSpan delay)
    {
        var hours = (decimal)Math.Ceiling(delay.TotalHours);
        return hours * _baseCosts.GetDepositFee();
    }

    public decimal CalculateDepositFee(Vehicle vehicle, int billableDays)
    {
        return _baseCosts.GetDepositFee() *  (CalculateBaseRate(vehicle, billableDays)/12);
    }
}

    