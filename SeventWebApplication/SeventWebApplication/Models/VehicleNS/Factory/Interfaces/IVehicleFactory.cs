namespace SeventWebApplication.Models.VehicleNS
{
    // Factory Method: varje fabrik vet hur den skapar sin fordonstyp.
    // TRequest gör att CarFactory bara tar CarCreateRequest och VanFactory bara VanCreateRequest,
    // så man kan inte råka skicka lastvolym till en personbil.
    public interface IVehicleFactory<in TRequest> where TRequest : VehicleCreateRequest
    {
        Vehicle Create(TRequest request);
    }
}