namespace SeventWebApplication.Models.Payment;

public class SwishPayment : IPayment
{
    public PaymentResult ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Swish payment processed: {amount} kr");

        return new PaymentResult
        {
            success = true,
            message = "Swish payment processed successfully"
        };
    }
}