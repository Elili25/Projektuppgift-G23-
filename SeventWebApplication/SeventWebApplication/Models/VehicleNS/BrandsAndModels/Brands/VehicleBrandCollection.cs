namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleBrandCollection : IVehicleBrandCollection
    {
        private readonly List<IVehicleBrand> _brands = new();

        public void Add(IVehicleBrand brand) => _brands.Add(brand);

        public IVehicleBrandIterator CreateIterator()
            => new VehicleBrandIterator(_brands);
    }
}
