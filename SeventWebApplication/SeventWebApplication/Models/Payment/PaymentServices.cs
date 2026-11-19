using SeventWebApplication.Models.Observers;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Payment;

// Betalning av hyrespriset (Booking.TotalPrice) med det betalsätt kunden valt (UC1).
// Betalningen ändrar INTE bokningens status. Det är personalens godkännande (Approve) som gör en
// bokning Confirmed, så att betalning inte kringgår godkännandet eller bilfördelningen.
// Själva betalleverantörerna är simulerade (Card/Swish/Invoice skriver bara ut i konsolen).
public class PaymentServices
{
    BookingNotifier _bookingNotifier;

    public PaymentServices( BookingNotifier bookingNotifier )
    {
        _bookingNotifier = bookingNotifier;
    }


    // Sparar valt betalsätt på bokningen och drar hyrespriset. Förseningsavgift och skadeavgift
    // tillkommer först vid retur och ingår därför inte här.
    public PaymentResult PayForBooking(BookingEntity booking, PaymentMethod method)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (booking.PaidAt is not null)
            return new PaymentResult { success = false, message = "The booking is already paid." };

        // Bokningen får betalsättet även om inget belopp kunde räknas ut (fordon utan prissättning)
        booking.PaymentMethod = method;

        
        if (booking.TotalPrice is null)
            return new PaymentResult { success = false, message = "The booking has no price, so it cannot be paid." };
        decimal amount = booking.TotalPrice.Value;

        IPayment payment = PaymentFactory.CreatePayment(method);
        PaymentResult result = payment.ProcessPayment(amount);

        if (result.success)
        {
            booking.PaidAmount = amount;
            booking.PaidAt = DateTime.Now;
            _bookingNotifier.Notify( new BookingEvent( BookingEventType.PaymentCompleted, booking ) );
        }
        else
            _bookingNotifier.Notify( new BookingEvent( BookingEventType.PaymentFailed, booking ) );

        return result;
    }

    // Betalar tillbaka det som betalats när bokningen avbokas. Kan bara göras en gång.
    // Statusen sätts av avbokningen (BookingManagementService), inte här.
    public bool RefundBooking(BookingEntity booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (booking.PaidAmount <= 0 || booking.RefundedAt is not null)
            return false;

        Console.WriteLine($"Refund processed: {booking.PaidAmount} kr ({booking.PaymentMethod})");
        booking.RefundedAt = DateTime.Now;
        return true;
    }
}
