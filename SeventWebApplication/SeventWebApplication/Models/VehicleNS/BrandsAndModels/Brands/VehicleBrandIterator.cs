namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleBrandIterator : IVehicleBrandIterator
    {
        private readonly List<IVehicleBrand> _brands;
        private int _position = 0;

        public VehicleBrandIterator(List<IVehicleBrand> brands)
        {
            _brands = brands;
        }

        public bool HasNext() => _position < _brands.Count;

        public IVehicleBrand Next() => _brands[_position++];

        public void Reset() => _position = 0;
    }
}
