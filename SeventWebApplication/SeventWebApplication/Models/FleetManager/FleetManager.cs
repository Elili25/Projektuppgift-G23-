
using SeventWebApplication.Controllers;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.VehicleNS;
using SeventWebApplication.Models.VehicleNS.Engine;
using System;
using System.Collections.ObjectModel;

public class FleetManager
{
    private static FleetManager? _instance = null;

    public static FleetManager instance
    {
        // Checks if the instance is not null and returns it or creates a new one if it is null
        get { return _instance ??= new FleetManager(); }
    }

    private Dictionary<string, Vehicle> _vehicles = new(StringComparer.OrdinalIgnoreCase);// <reg-num, vehicle>

    // Bokningarna och tillhörande logik (_bookings, RegisterBooking, GetBookings) är nu flyttat till BookingRegistry

    // Observer: meddelar de som lyssnar (t.ex. VehicleAssignmentService) att en ny bil lagts till.
    // FleetManager behöver därmed inte känna till fordonsfamiljer, och vi undviker ett cirkulärt beroende.
    public event Action<Vehicle>? VehicleAdded;

    // Private constructor so only one instance can be created (using the instance getter)
    private FleetManager() { }

    // Registers a vehicle in the fleet, keyed by its registration number.
    // Format validation per country is done upstream (IRegNrValidator in the vehicle factories).
    public void AddVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        var regNr = vehicle.RegistrationPlate.RegNr;

        if (_vehicles.ContainsKey(regNr))
            throw new InvalidOperationException($"A vehicle with registration number {regNr} already exists.");

        _vehicles.Add(regNr, vehicle);

        // Sker efter att bilen är sparad, så lyssnarna ser den i flottan
        VehicleAdded?.Invoke(vehicle);
    }

    public IEnumerable<Vehicle> GetAvailableVehicles(DateTime startTime, DateTime endTime)
    {
        var availableVehicles = new List<Vehicle>();

        foreach (var vehicle in _vehicles.Values)
        {
            if (IsAvailable(vehicle, startTime, endTime))
            {
                availableVehicles.Add(vehicle);
            }
        }

        return availableVehicles;
    }

    // Returns a list of Vehicles based on a query collection (probably from a HttpRequest)
    public IEnumerable<Vehicle> FindVehiclesFromQuery( IQueryCollection query )
    {
        List<Vehicle> results = new();

        // replace okt -> oct för att parsing ska fungera
        string datePickup = $"{query["datePickup"].ToString().Replace( 'k', 'c' )} {query["timePickup"].ToString()}";
        string dateReturn = $"{query["dateReturn"].ToString().Replace( 'k', 'c' )} {query["timeReturn"].ToString()}";

        DateTime pickupDate = DateTime.Parse( $"2026 {datePickup}" );
        DateTime returnDate = DateTime.Parse( $"2026 {dateReturn}" );

        Console.WriteLine( pickupDate );
        Console.WriteLine( returnDate );

        foreach( Vehicle v in GetAvailableVehicles( pickupDate, returnDate ) )
        {
            results.Add(v);
        }

        return results;
    }

    // NYTT: Söker fordon utifrån bilmärke eller modell.
    // Används av HomeController när söktext skickas in som query.
    public IEnumerable<Vehicle> FindVehicles(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Vehicle>();

        return _vehicles.Values.Where(vehicle =>
            vehicle.Model.Brand.BrandName.Contains(
                query, StringComparison.OrdinalIgnoreCase)
            ||
            vehicle.Model.ModelName.Contains(
                query, StringComparison.OrdinalIgnoreCase));
    }


    // Teoretiskt så skulle man kunna låta FleetManager spara de upptagna perioderna som en observer sedan uppdaterar
    // (t.ex. vid BookingConfirmedEvent/BookingCancelledEvent), och så kollar IsAvailable endast på
    // de perioderna istället. Då slipper man att loopa alla bokningar varje gång och det blir lösare koppling
    // till BookingRegistry, men det är mest bara en tanke, inget vi behöver greja med utan keep it simple lol.
    public bool IsAvailable(Vehicle vehicle, DateTime startDate, DateTime endDate)
    {
        // Two periods overlap when each one starts before the other ends.
        return !BookingRegistry.Instance.GetBookingsByVehicleId(vehicle.VehicleId)
            .Any(b => b.Status != BookingStatus.Cancelled && b.StartDate < endDate && startDate < b.EndDate);
    }

    // TESTDATA: Skapar en dummybil för att testa sökfunktionen.
    public void SeedTestVehicle()
    {
        var brand = new VehicleBrand("Volvo");

        var model = new VehicleModel("V60", brand.BrandId)
        {
            Brand = brand
        };

        var registrationPlate = new RegistrationPlate(
            "ABC123",
            CountryCode.SE
        );

        // Skapa motorn separat
        IEngine engine = new PetrolEngine(
            197,  // HorsePower
            50    // FuelCapacityLiters
        );

        var vehicle = new Car(
            registrationPlate,
            model,
            VehicleCategoryType.PassengerCar,
            SeventWebApplication.Models.VehicleNS.PassengerCarCategoryType.StationWagon,
            SeventWebApplication.Models.VehicleNS.VehicleComfortType.Economy,
            2024,
            5,
            529m,
            engine,
            TransmissionType.Automatic
        );

        // Anropas vid varje besök på startsidan, så hoppa över om testbilen redan finns.
        // Går via AddVehicle (inte direkt mot _vehicles) så att VehicleAdded skickas och bilen kopplas till en familj.
        if (!_vehicles.ContainsKey(registrationPlate.RegNr))
            AddVehicle(vehicle);
    }

    public record VehicleSearchResult
    {
        public VehicleSearchResult( string brand, string model )
        {
            Brand = brand;
            Model = model;
        }

        public VehicleSearchResult( Vehicle vehicle )
        {
            Brand = vehicle.Model.Brand.BrandName;
            Model = vehicle.Model.ModelName;
        }

        // ÄNDRING: Egenskaper istället för publika fält,
        // så att JSON-serialiseringen inkluderar värdena.
        public string Brand { get; init; }
        public string Model { get; init; }
    }
    //för families 
    public IEnumerable<Vehicle> GetAllVehicles()
    {
        return _vehicles.Values.ToList();
    }

    public Vehicle? GetVehicle(Guid id)
    {
        return _vehicles.Values.FirstOrDefault(vehicle => vehicle.VehicleId == id);
    }
}
