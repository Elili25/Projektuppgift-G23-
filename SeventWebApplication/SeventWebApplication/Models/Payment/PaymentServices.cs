using SeventWebApplication.Models.Observers;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Payment;

// Betalning av hyrespriset (Booking.TotalPrice) med det betalsätt kunden valt (UC1),
// och av avgifterna som tillkommer vid retur (förseningsavgift + skadeavgift).
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
    // tillkommer först vid retur och ingår därför inte här (se PayOutstandingFees).
    public PaymentResult PayForBooking(BookingEntity booking, PaymentMethod method)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (booking.PaidAt is not null)
            return new PaymentResult { success = false, message = "The booking is already paid." };

        if (booking.TotalPrice is null)
            return new PaymentResult { success = false, message = "The booking has no price, so it cannot be paid." };

        // Bokningen får betalsättet registrerat
        booking.PaymentMethod = method;

        decimal amount = booking.TotalPrice.Value;
        if (amount <= 0)
            return new PaymentResult { success = true, message = "Nothing to pay." };

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

    // Kunden betalar avgifterna som tillkom vid retur.
    // Beloppet kommer alltid från bokningen (FeesOutstanding), aldrig från anroparen.
    public PaymentResult PayOutstandingFees(BookingEntity booking, PaymentMethod method)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (!Enum.IsDefined(method))
            return new PaymentResult { success = false, message = "Choose a payment method." };

        decimal due = booking.FeesOutstanding;
        if (due <= 0)
            return new PaymentResult { success = false, message = "There are no outstanding fees on this booking." };

        IPayment payment = PaymentFactory.CreatePayment(method);
        PaymentResult result = payment.ProcessPayment(due);

        if (result.success)
        {
            _bookingNotifier.Notify( new BookingEvent( BookingEventType.FeesPaymentCompleted, booking ) );
        }
        else
            _bookingNotifier.Notify( new BookingEvent( BookingEventType.FeesPaymentFailed, booking ) );

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
