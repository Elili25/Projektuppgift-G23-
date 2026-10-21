using SeventWebApplication.Models.Addon;

namespace SeventWebApplication.Models.BookingValidation
{
    //public class AddonValidationStrategy : IBookingValidationStrategy
    //{
    //    public void Validate(BookingBuilder booking)
    //    {
    //        IAddon addon = booking.Addon;
    //        if (addon == null) return;

    //        int gpsCount = CountAddonType<GPSAddon>(addon);
    //        int extraDriverCount = CountAddonType<ExtraDriverAddon>(addon);
    //        int childSeatCount = CountAddonType<ChildSeatAddon>(addon);

    //        if (gpsCount > 1)
    //            throw new InvalidOperationException("Du kan max välja 1 GPS.");

    //        if (extraDriverCount > 1)
    //            throw new InvalidOperationException("Du kan max välja 1 extra förare.");

    //        if (childSeatCount > 3)
    //            throw new InvalidOperationException("Du kan max välja 3 barnstolar.");
    //    }

    //    private int CountAddonType<T>(IAddon addon) where T : IAddon  // Tar emot ett IAddon och räknar rekursivt hur många av typen T som finns i kedjan
    //    {
    //        // Om vi har nått slutet av kedjan, returneras 0
    //        if (addon == null) return 0;

    //        int count = 0;  // Lokal räknare för det aktuella objektet i kedjan
    //        if (addon is T) count++;  // Om det aktuella objektet matchar den sökta typen T, öka räknaren med 1

    //        if (addon is BookingValidationComposite decorator)  // Om objektet är en dekoratör finns det flera länkar i kedjan
    //        {
    //            // Metoden anropar sig själv, vilket skapar en rekursiv "loop" och
    //            // Återvänder till metodens början
    //            count += CountAddonType<T>(decorator.GetWrappedAddon());  // Plussar ihop resulatet för alla iterationer
    //        }
    //        return count;  // Returnerar slutsumman
    //    }
    //}

}
