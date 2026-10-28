using SeventWebApplication.Models.Booking;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.VehicleNS
{
    public class RentalReturnService
    {
        private readonly BookingRepository _bookingRepository;
        private readonly VehicleRentalRecordRepository _recordRepository;
        private readonly PricingService _pricingService;

        public RentalReturnService(
            BookingRepository bookingRepository,
            VehicleRentalRecordRepository recordRepository,
            PricingService pricingService)
        {
            _bookingRepository = bookingRepository;
            _recordRepository = recordRepository;
            _pricingService = pricingService;
        }

        public void RegisterPickup(
            string bookingId,
            int mileage,
            decimal fuelLevel,
            decimal batteryLevel)
        {
            var booking = GetBooking(bookingId);

            if (_recordRepository.GetByBookingId(bookingId) != null)
                throw new InvalidOperationException("Pickup has already been registered.");

            if (booking.Status != "Approved")
                throw new InvalidOperationException("Booking must be approved before pickup.");

            if (!booking.Vehicle.IsAvailable)
                throw new InvalidOperationException("Vehicle is not available.");

            if (mileage < 0)
                throw new ArgumentOutOfRangeException(nameof(mileage));

            var record = new VehicleRentalRecord
            {
                BookingId = bookingId,
                ActualPickupDate = DateTime.Now,
                PickupMileage = mileage,
                PickupFuelLevel = fuelLevel,
                PickupBatteryLevel = batteryLevel
            };

            _recordRepository.Add(record);

            booking.Vehicle.IsAvailable = false;
            booking.Status = "Active";
        }

        public void RegisterReturn(
            string bookingId,
            int mileage,
            decimal? fuelLevel,
            decimal? batteryLevel)
        {
            var booking = GetBooking(bookingId);
            var record = GetRecord(bookingId);

            if (record.IsReturned())
                throw new InvalidOperationException("Return has already been registered.");

            if (mileage < record.PickupMileage)
                throw new ArgumentException("Return mileage cannot be lower than pickup mileage.");

            record.ActualReturnDate = DateTime.Now;
            record.ReturnMileage = mileage;
            record.ReturnFuelLevel = fuelLevel;
            record.ReturnBatteryLevel = batteryLevel;

            _recordRepository.Update(record);

            booking.Vehicle.IsAvailable = true;
            booking.Status = "Completed";
        }

        public decimal CalculateLateFee(string bookingId)
        {
            var delay = GetDelay(bookingId);

            return delay > TimeSpan.Zero
                ? _pricingService.CalculateLateFee(bookingId, delay)
                : 0m;
        }

        public bool IsLateReturn(string bookingId)
        {
            return GetDelay(bookingId) > TimeSpan.Zero;
        }

        private BookingEntity GetBooking(string bookingId)
        {
            return _bookingRepository.GetById(bookingId)
                ?? throw new InvalidOperationException("Booking not found.");
        }

        private VehicleRentalRecord GetRecord(string bookingId)
        {
            return _recordRepository.GetByBookingId(bookingId)
                ?? throw new InvalidOperationException("No pickup record exists for this booking.");
        }

        private TimeSpan GetDelay(string bookingId)
        {
            var record = _recordRepository.GetByBookingId(bookingId);
            if (record == null || !record.IsReturned())
                return TimeSpan.Zero;

            var booking = _bookingRepository.GetById(bookingId);
            if (booking == null)
                return TimeSpan.Zero;

            return record.CalculateDelay(booking.EndDate);
        }

        //placeholder tills tykos kommer in
        public class PricingService
        {
            private const decimal LateFeePerHour = 100m;   // justera efter behov

            public decimal CalculateLateFee(string bookingId, TimeSpan delay)
            {
                var hours = (decimal)Math.Ceiling(delay.TotalHours);
                return hours * LateFeePerHour;
            }
        }
    }
}