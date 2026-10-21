
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Repositories
{
    public class VehicleModelRepository
    {
        private readonly List<VehicleModel> _models = new();
        private readonly VehicleBrandRepository _brandRepository;

        public VehicleModelRepository(VehicleBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public void Add(VehicleModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var brand = _brandRepository.GetById(model.BrandId);

            if (brand == null)
            {
                throw new InvalidOperationException(
                    $"Det finns inget bilmärke med ID {model.BrandId}.");
            }

            if (_models.Any(m => m.ModelId == model.ModelId))
            {
                throw new InvalidOperationException(
                    "Modellen finns redan i registret.");
            }

            _models.Add(model);
        }

        public VehicleModel? GetById(Guid modelId)
        {
            return _models.FirstOrDefault(m => m.ModelId == modelId);
        }

        public IEnumerable<VehicleModel> GetAll()
        {
            return _models.AsReadOnly();
        }

        public IEnumerable<VehicleModel> GetByBrandId(Guid brandId)
        {
            return _models.Where(m => m.BrandId == brandId).ToList();
        }

        public void Remove(Guid modelId)
        {
            var model = GetById(modelId);

            if (model != null)
                _models.Remove(model);
        }

        public void Seed()
        {
            if (_models.Count > 0)
                return;

            SeedBrandModels("Volvo", "V60", "XC60", "XC90");
            SeedBrandModels("BMW", "3 Series", "5 Series", "X5");
            SeedBrandModels("Audi", "A3", "A4", "Q5");
            SeedBrandModels("Volkswagen", "Golf", "Passat", "Tiguan");
            SeedBrandModels("Toyota", "Corolla", "Yaris", "RAV4");
            SeedBrandModels("Mercedes-Benz", "C-Class", "E-Class", "GLC");
        }

        private void SeedBrandModels(string brandName, params string[] modelNames)
        {
            var brand = _brandRepository.GetAll()
                .FirstOrDefault(b =>
                    string.Equals(
                        b.BrandName,
                        brandName,
                        StringComparison.OrdinalIgnoreCase));

            if (brand == null)
            {
                throw new InvalidOperationException(
                    $"Bilmärket '{brandName}' måste seedas innan modellerna.");
            }

            foreach (var modelName in modelNames)
            {
                var model = new VehicleModel(modelName, brand.BrandId)
                {
                    Brand = brand
                };
            }
        }
    }
}
