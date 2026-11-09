using SeventWebApplication.Models.Booking;

namespace SeventWebApplication.Models.BookingValidation
{
    public interface IValidationStrategy
    {
        // Returnerar en samling fel
        // Använder IEnumerable för att dölja datastrukturen och förhindra ändringar med Add() & Remove()
        IEnumerable<ValidationError> Validate(BookingRequest request);
    }
}
