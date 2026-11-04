namespace SeventWebApplication.Models.Payment;

public class PaymentServices
{
    public PaymentResult PayForBooking(
        Booking.Booking booking,
        PaymentMethod method)
    {
        IPayment payment = PaymentFactory.CreatePayment(method);

        PaymentResult result =
            payment.ProcessPayment(100); //Ska fixa booking delen så att den integrerar med pris och betalning, här ska booking.TotalPrice vara

        if (result.success)
        {
            booking.Status = "true";
        }

        return result;
    }
}