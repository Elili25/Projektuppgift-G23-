namespace SeventWebApplication.Models.Payment;

public class CardPayment : IPayment
{
    public PaymentResult ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Card payment processed: {amount} kr");

        return new PaymentResult
        {
            success = true,
            message = "Card payment processed successfully"
        };
    }
}