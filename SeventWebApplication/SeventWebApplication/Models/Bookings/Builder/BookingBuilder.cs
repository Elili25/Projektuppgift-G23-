
namespace SeventWebApplication.Models.BookingBuilder
{
    public class BookingBuilder
    {
        private Guid _userId;
        private DateTime _startDate;
        private DateTime _endDate;

        //private VehiclecategoryType? _vehicleCategoryType;
        //Denna connectar ej då vi inte har designat hur vi ska koppla till VehicleCategoryType, priceClass, transmissionType, fuelType, etc.

        // TO DO: Lägg till fordonsconnection när vi har designat hur vi ska koppla det. 
        private readonly List<Addon> _addons = new();
        private IPricingStrategy _pricing;
        private string? _notes;

        public BookingBuilder ForUser(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public BookingBuilder FromTo(DateTime start, DateTime end)
        {
            _startDate = start;
            _endDate = end;
            return this;
        }
        public BookingBuilder AddAddon(Addon addon)
        {
            _addons.Add(addon);
            return this;
        }
        public BookingBuilder WithPricing(IPricingStrategy pricing)
        {
            _pricing = pricing;
            return this;
        }
        public BookingBuilder WithNotes(string? notes)
        {
            _notes = notes;
            return this;
        }

        public Booking Build()
        {
            var booking = new Booking
            {
                UserId = _userId,
                BookingId = Guid.NewGuid(),
                StartDate = _startDate,
                EndDate = _endDate,
                Addons = _addons.ToList().AsReadOnly(),
                PricingStrategy = _pricing,
                Notes = _notes,
                Status = BookingStatus.Pending
            };

            return booking;

        }
    }
}



//public BookingBuilder WithVehicle(Vehicle v)
//{
//	_vehicleCategoryType = v.CategoryType;
//	return this;
//}
//Denna connectar ej då vi inte har designat hur vi ska koppla till VehicleCategoryType, priceClass, transmissionType, fuelType, etc.