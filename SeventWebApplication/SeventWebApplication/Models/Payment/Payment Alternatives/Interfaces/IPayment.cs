namespace SeventWebApplication.Models.Payment;

public interface IPayment
{
    public PaymentResult ProcessPayment(decimal amount);
}