
namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleBrandRepository : IVehicleBrandRepository
    {
        private readonly List<VehicleBrand> _brands = new();

        public VehicleBrand? GetById(Guid id)
        {
            return _brands.FirstOrDefault(b => b.BrandId == id);
        }

        public IEnumerable<VehicleBrand> GetAll()
        {
            return _brands.AsReadOnly();
        }

        public void Add(VehicleBrand brand)
        {
            ArgumentNullException.ThrowIfNull(brand);

            if (_brands.Any(b =>
                string.Equals(
                    b.BrandName,
                    brand.BrandName,
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "Märket finns redan.");
            }

            _brands.Add(brand);
        }

        public void Seed()
        {
            if (_brands.Count > 0)
                return;

            Add(new VehicleBrand("Volvo"));
            Add(new VehicleBrand("BMW"));
            Add(new VehicleBrand("Audi"));
            Add(new VehicleBrand("Volkswagen"));
            Add(new VehicleBrand("Toyota"));
            Add(new VehicleBrand("Mercedes-Benz"));
        }
    }
}
