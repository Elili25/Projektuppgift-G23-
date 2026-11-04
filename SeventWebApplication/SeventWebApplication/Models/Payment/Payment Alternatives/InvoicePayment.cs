namespace SeventWebApplication.Models.Payment;

public class InvoicePayment : IPayment
{
    public PaymentResult ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Invoice payment processed: {amount} kr");

        return new PaymentResult
        {
            success = true,
            message = "Invoice payment processed successfully"
        };
    }
}