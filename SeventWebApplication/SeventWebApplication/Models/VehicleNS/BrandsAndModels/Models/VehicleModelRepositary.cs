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

            // Kontrollera att märket finns, precis som en foreign key
            var brand = _brandRepository.GetById(model.BrandId);

            if (brand == null)
            {
                throw new InvalidOperationException(
                    $"Det finns inget bilmärke med ID {model.BrandId}.");
            }

            // Förhindra dubbletter av modell-ID
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
            return _models
                .Where(m => m.BrandId == brandId)
                .ToList();
        }

        public void Remove(Guid modelId)
        {
            var model = GetById(modelId);

            if (model != null)
            {
                _models.Remove(model);
            }
        }
    }
}