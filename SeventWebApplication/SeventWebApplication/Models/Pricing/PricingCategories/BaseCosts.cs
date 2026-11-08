using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Pricing.PricingCategories;

public class BaseCosts
{
    private decimal _baseFee = 50;
    
    public decimal GetBaseFee()
    {
        return _baseFee;
    }

    private decimal _baseDepositFee = 100;
    
    public decimal GetDepositFee()
    {
        return _baseDepositFee; 
    }
    public void setDepositFee(decimal value)
    {
        _baseDepositFee = value;
    }
}