namespace SeventWebApplication.Models.Booking
{
    // Lagrar alla bokningar. BookingRegistry äger den enda instansen.
    public class BookingRepository
    {
        private readonly List<Booking> _bookings = new();

        // Adds a new booking to the repository
        public void Add(Booking booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            if (_bookings.Any(b => b.BookingId == booking.BookingId))
                throw new InvalidOperationException("Booking already exists.");

            _bookings.Add(booking);
        }

        // Retrieves a booking by its ID
        public Booking? GetById(string bookingId)
            => _bookings.FirstOrDefault(b => b.BookingId == bookingId);

        // Retrieves all bookings for a specific user
        public IEnumerable<Booking> GetByUserId(string userId)
            => _bookings.Where(b => b.UserId == userId).ToList();

        // Retrieves all bookings for a specific vehicle
        public IEnumerable<Booking> GetByVehicleId(Guid vehicleId)
            => _bookings.Where(b => b.Vehicle != null && b.Vehicle.VehicleId == vehicleId).ToList();

        // Retrieves all bookings with a specific status
        public IEnumerable<Booking> GetByStatus(string status)
            => _bookings.Where(b => b.Status == status).ToList();

        // Retrieves all bookings
        public IEnumerable<Booking> GetAll()
            => _bookings.ToList();

        // Updates an existing booking in the repository
        public void Update(Booking booking)
        {
            var index = _bookings.FindIndex(b => b.BookingId == booking.BookingId);
            if (index < 0)
                throw new InvalidOperationException("Booking not found.");

            _bookings[index] = booking;
        }

        // Removes a booking from the repository by its ID
        public bool Remove(string bookingId)
            => _bookings.RemoveAll(b => b.BookingId == bookingId) > 0;
    }
}