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
            booking.Status = Booking.BookingStatus.Confirmed;
        }

        return result;
    }

    public void RefundBooking(Booking.Booking booking)
    {
        if(booking.Status == Booking.BookingStatus.Confirmed && DateTime.Now < (booking.StartDate - TimeSpan.FromHours(8))) //Vet inte hur långt i förväg man ska kunna avboka, nu är det 8 timmar innan starttiden
        {
            booking.Status = Booking.BookingStatus.Cancelled;
        }
    }
}
