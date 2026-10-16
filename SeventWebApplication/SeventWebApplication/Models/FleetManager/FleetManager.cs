using System;

public class FleetManager
{
	private static FleetManager? _instance;

    public static FleetManager instance
    {
        // Checks if the instance is not null and returns it or creates a new one if it is null
        get { return _instance ??= new FleetManager(); }
    }

    private Dictionary<string, int /*Vehicle*/> _vehicles = new();// <reg-num, vehicle>
    private List< int /*Booking*/> _bookings = new();

    // Private constructor so only one instance can be created (using the instance getter)
    private FleetManager(){}

    // Vet inte om den borde ta in en Vehicle eller delar av en Vehicle, som reg-num, typ, etc
    //public void AddVehicle(Vehicles vehicle)
    //{
    //    if(vehicle.regnum.Count() != 6 || vehicle.regnum.Count() != 7) 
    //    {
    //        // Ge en varning att det inte är ett svenskt regnummer?
    //    }
    //    _vehicles.Add(vehicle.RegistrationNumber, vehicle);
    //}

    //public List<Vehicles> GetAvailableVehicles(DateTime startTime, DateTime endTime)
    //{
    //    var availableVehicles = new List<Vehicles>();

    //    foreach (var vehicle in _vehicles)
    //    {
    //        if (IsAvailable(vehicle, startTime, endTime))
    //        {
    //            availableVehicles.Add(vehicle);
    //        }
    //    }
   
    //    return availableVehicles;
    //}

    //public bool IsAvailable(Vehicle vehicle, DateTime startTime, DateTime endTime)
    //{
    //    foreach(var booking in _bookings)
    //    {
    //        if (booking.vehicle.regnum != vehicle.regnum)
    //            continue

    //        // Checks if startTime is inside the booking
    //        if (startTime.CompareTo(booking.startTime) > 0 && startTime.CompareTo(booking.endTime) < 0)
    //            return false;
    //        // Checks if endTime is inside the booking
    //        if (endTime.CompareTo(booking.startTime) > 0 && endTime.CompareTo(booking.endTime) < 0)
    //            return false;
    //        // If startTime is before the booking startTime AND endTime is after the booking endTime
    //        if (startTime.CompareTo(booking) < 0 && endTime.CompareTo(booking.endTime) > 0)
    //            return false;

    //    }
    //    return true;
    //}

    //public void RegisterBooking(Booking booking)// vill nog ha några andra inputs?
    //{
    //    // TODO: Implementera när booking är klar

    //    _bookings.Add(booking)//Temp
    //}
}
