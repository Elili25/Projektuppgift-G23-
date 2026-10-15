namespace SeventWebApplication.Models.VehicleNs
{
    public interface IVehicleBrandIterator
    {
        bool HasNext();
        IVehicleBrand Next();
        void Reset();
    }
}
