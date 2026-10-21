namespace SeventWebApplication.Models.VehicleNS
{
    public interface IVehicleBrandCollection
    {
        IVehicleBrandIterator CreateIterator();
        void Add(IVehicleBrand brand);
    }
}
