using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Booking
{
    public static class VehicleAssignmentService
    {
        // Creats a VehicleAssignment based of the query collection from a Http request when searching the website
        public static VehicleAssignment AssignVehiclesFromQuery( IQueryCollection query )
        {
            VehicleAssignment assignment = new VehicleAssignment(
                    FleetManager.instance.FindVehiclesFromQuery( query ),
                    query
                    );

            return assignment;
        }
    }

  
}
