using System;

public class FleetManager
{
	private static FleetManager _instance;

	public static FleetManager instance
    {
        // Returns the instance or creates a new one if it doesn't exist
        get{ return _instance != null ? _instance : _instance = new FleetManager(); }
    }

    private Dictionary<string, Vehicle> _vehicles;// <reg-num, vehicle>
    private List<Booking> _bookings;

    // Dont
    private FleetManager(){}

    // Vet inte om den borde ta in en Vehicle eller delar av en Vehicle, som reg-num, typ, etc
    public void AddVehicle(Vehicles vehicle)
    {
        if(vehicle.regnum.Count() != 6 || vehicle.regnum.Count() != 7) 
        {
            // Ge en varning att det inte är ett svenskt regnummer?
        }
        _vehicles.Add(vehicle.RegistrationNumber, vehicle);
    }

    public List<Vehicles> GetAvailableVehicles(DateTime startTime, DateTime endTime)
    {
        var availableVehicles = new List<Vehicles>();

        foreach (var vehicle in _vehicles)
        {
            if (IsAvailable(vehicle, startTime, endTime))
            {
                availableVehicles.Add(vehicle);
            }
        }
   
        return availableVehicles;
    }

    public bool IsAvailable(Vehicle vehicle, DateTime startTime, DateTime endTime)
    {
        
    }

    public void RegisterBooking(Booking booking)
    {
        //TODO: Implementera när booking är klar
    }
}
