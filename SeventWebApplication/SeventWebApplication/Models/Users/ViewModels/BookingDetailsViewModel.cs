namespace SeventWebApplication.Models.Users
{
    // Allt som detaljsidan för en bokning (/Bookings/Details/{id}) visar.
    // Raden återanvänds från listan, så siffror och namn alltid stämmer överens med den.
    public class BookingDetailsViewModel
    {
        public BookingRowViewModel Booking { get; init; } = new();
        public bool IsStaff { get; init; }

        // Vald fordonsklass när bokningen gjorts på en familj (null om en specifik bil valdes)
        public string? FamilyName { get; init; }

        public string? Notes { get; init; }
    }
}
