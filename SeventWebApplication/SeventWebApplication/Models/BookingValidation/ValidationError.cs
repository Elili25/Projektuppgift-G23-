namespace SeventWebApplication.Models.BookingValidation
{
    public class ValidationError
    {
        public required string Property { get; set; } // Håller koll på vart felet uppstod, t.ex "Start" eller "Addons"
        public required string Message { get; set; }  // Felmeddelandet, t.ex. "Startdatumet kan inte ha passerat."
    }

}
