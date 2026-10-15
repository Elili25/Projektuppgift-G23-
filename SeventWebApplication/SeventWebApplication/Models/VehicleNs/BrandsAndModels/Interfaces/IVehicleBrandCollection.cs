namespace SeventWebApplication.Models.VehicleNs
{
    public interface IVehicleBrandCollection
    {
        IVehicleBrandIterator CreateIterator();
        void Add(IVehicleBrand brand);
    }
}
