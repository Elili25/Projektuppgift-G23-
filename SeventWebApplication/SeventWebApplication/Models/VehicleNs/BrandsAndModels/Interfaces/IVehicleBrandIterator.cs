namespace SeventWebApplication.Models.VehicleNS
{
    public interface IVehicleBrandIterator
    {
        bool HasNext();
        IVehicleBrand Next();
        void Reset();
    }
}
