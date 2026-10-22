using SeventWebApplication.Models.VehicleNS;
using static FleetManager;

namespace SeventWebApplication.Models.Booking
{
    public class VehicleAssignment
    {

        public VehicleAssignment() { }
        public VehicleAssignment( IEnumerable<Vehicle> vehicles, IQueryCollection query ) 
        {
            _vehicles = vehicles;
            _query = query;
        }

        public IEnumerable<Vehicle>? _vehicles;
        public IQueryCollection? _query;
    }
}
