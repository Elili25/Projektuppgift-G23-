namespace SeventWebApplication.Models.VehicleNS
{
    public interface IVehicleBrandRepository
    {
        VehicleBrand? GetById(Guid id);
        IEnumerable<VehicleBrand> GetAll();
        void Add(VehicleBrand brand);
    }
}
