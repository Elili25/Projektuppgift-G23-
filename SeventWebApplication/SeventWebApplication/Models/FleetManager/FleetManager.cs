
using SeventWebApplication.Controllers;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Maintenance;
using SeventWebApplication.Models.VehicleNS;
using SeventWebApplication.Models.VehicleNS.Engine;
using System;
using System.Collections.ObjectModel;

public class FleetManager
{
    private static readonly Lazy<FleetManager> _instance = new(() => new FleetManager());

    public static FleetManager instance => _instance.Value;

    private Dictionary<string, Vehicle> _vehicles = new(StringComparer.OrdinalIgnoreCase);// <reg-num, vehicle>

    // Underhållsperioder (service/reparation). Håller koll på när en bil är ur drift.
    private readonly List<MaintenancePeriod> _maintenancePeriods = new();

    // Bokningarna och tillhörande logik (_bookings, RegisterBooking, GetBookings) är nu flyttat till BookingRegistry

    // Observer: meddelar de som lyssnar (t.ex. VehicleAssignmentService) att en ny bil lagts till.
    // FleetManager behöver därmed inte känna till fordonsfamiljer, och vi undviker ett cirkulärt beroende.
    public event Action<Vehicle>? VehicleAdded;

    // Skickas när en bil tagits bort, så att t.ex. familjekopplingen kan städas
    public event Action<Vehicle>? VehicleRemoved;

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

    // Registrerar en underhållsperiod för en bil. EndDate = null betyder pågående underhåll.
    public void AddMaintenancePeriod(MaintenancePeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);

        if (GetVehicle(period.VehicleId) == null)
            throw new InvalidOperationException($"Vehicle {period.VehicleId} not found.");

        if (_maintenancePeriods.Any(p => p.MaintenanceId == period.MaintenanceId))
            throw new InvalidOperationException($"Maintenance period {period.MaintenanceId} already exists.");

        _maintenancePeriods.Add(period);
    }

    // Avslutar en underhållsperiod genom att sätta slutdatum.
    public void EndMaintenancePeriod(Guid maintenanceId, DateTime endDate)
    {
        int index = _maintenancePeriods.FindIndex(p => p.MaintenanceId == maintenanceId);

        if (index < 0)
            throw new KeyNotFoundException($"Maintenance period {maintenanceId} not found.");

        _maintenancePeriods[index] = _maintenancePeriods[index] with { EndDate = endDate };
    }

    // Pågår det underhåll för bilen just nu? (EndDate = null: tillsvidare)
    public bool IsInMaintenanceNow(Guid vehicleId)
    {
        var now = DateTime.UtcNow;
        return _maintenancePeriods.Any(p => p.VehicleId == vehicleId
            && p.StartDate <= now
            && (p.EndDate == null || p.EndDate.Value > now));
    }

    public IEnumerable<MaintenancePeriod> GetMaintenancePeriods(Guid vehicleId)
        => _maintenancePeriods.Where(p => p.VehicleId == vehicleId).ToList();

    // Uppdaterar en bils beskrivande fält. Dictionaryn är nycklad på regnummer, så vid nytt regnummer byts nyckeln.
    public void UpdateVehicle(Guid id, RegistrationPlate plate, VehicleModel model, int year, int passengerCapacity, decimal luggageCapacity)
    {
        var vehicle = GetVehicle(id) ?? throw new InvalidOperationException("Vehicle not found.");
        var oldKey = vehicle.RegistrationPlate.RegNr;

        if (!string.Equals(oldKey, plate.RegNr, StringComparison.OrdinalIgnoreCase) && _vehicles.ContainsKey(plate.RegNr))
            throw new InvalidOperationException($"A vehicle with registration number {plate.RegNr} already exists.");

        vehicle.UpdateDetails(plate, model, year, passengerCapacity, luggageCapacity);

        _vehicles.Remove(oldKey);
        _vehicles[plate.RegNr] = vehicle;
    }

    // Tar bort en bil. Blockeras om bilen är uthyrd eller har pågående/kommande bokningar.
    public void RemoveVehicle(Guid id)
    {
        var vehicle = GetVehicle(id) ?? throw new InvalidOperationException("Vehicle not found.");

        if (!vehicle.IsAvailable)
            throw new InvalidOperationException("The vehicle is rented out right now and cannot be deleted.");

        int openBookings = BookingRegistry.Instance.GetBookingsByVehicleId(id)
            .Count(b => b.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.Active);

        if (openBookings > 0)
            throw new InvalidOperationException($"The vehicle has {openBookings} pending, confirmed or active booking(s) and cannot be deleted. Cancel or complete them first.");

        _vehicles.Remove(vehicle.RegistrationPlate.RegNr);
        VehicleRemoved?.Invoke(vehicle);
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
        if (!vehicle.IsOperational)
            return false;

        // Underhåll blockerar bilen under perioden. Underhållets datum är UTC, bokningarnas är lokala,
        // så vi jämför i lokal tid (samma som VehicleMaintenanceService.EnsureNotRentedDuring). EndDate = null: pågående, tillsvidare.
        bool inMaintenance = _maintenancePeriods.Any(p =>
            p.VehicleId == vehicle.VehicleId
            && p.StartDate.ToLocalTime() < endDate
            && (p.EndDate == null || startDate < p.EndDate.Value.ToLocalTime()));

        if (inMaintenance)
            return false;

        return !BookingRegistry.Instance.GetBookingsByVehicleId(vehicle.VehicleId)
            .Any(b => b.Status != BookingStatus.Cancelled
            && b.Status != BookingStatus.Completed
            && b.StartDate < endDate && startDate < b.EndDate);
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
