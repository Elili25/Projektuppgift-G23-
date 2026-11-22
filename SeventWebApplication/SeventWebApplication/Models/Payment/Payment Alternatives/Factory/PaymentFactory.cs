namespace SeventWebApplication.Models.Payment;

public static class PaymentFactory
{
    public static IPayment CreatePayment(PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethod.Card => new CardPayment(),
            PaymentMethod.Invoice => new InvoicePayment(),
            PaymentMethod.Swish => new SwishPayment(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}