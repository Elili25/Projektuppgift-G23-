using SeventWebApplication.Repositories;

namespace SeventWebApplication.Models.VehicleNS
{
    // Märke + modeller för visning i listor
    public record CatalogBrand(string Name, List<string> Models);

    // Registret över tillåtna märke/modell-kombinationer. Fleet/Create väljer ur det, och admin lägger till nya
    // kombinationer på dashboarden. Märkeslistan går igenom VehicleBrandCollection/VehicleBrandIterator.
    public class VehicleCatalogService
    {
        private readonly VehicleBrandRepository _brands;
        private readonly VehicleModelRepository _models;
        private readonly object _lock = new();

        public VehicleCatalogService(VehicleBrandRepository brands, VehicleModelRepository models)
        {
            _brands = brands;
            _models = models;
        }

        // Seedar standardmärkena och lägger till kombinationerna för bilar som redan finns i flottan
        public void Seed(IEnumerable<Vehicle> existingVehicles)
        {
            lock (_lock)
            {
                _brands.Seed();
                _models.Seed();

                foreach (var v in existingVehicles)
                    TryAdd(v.Model.Brand.BrandName, v.Model.ModelName, v is Van ? VehicleType.Van : VehicleType.Car);
            }
        }

        // Märkena i bokstavsordning, hämtade med iteratorn
        public IVehicleBrandIterator CreateBrandIterator()
        {
            var collection = new VehicleBrandCollection();
            foreach (var brand in _brands.GetAll().OrderBy(b => b.BrandName, StringComparer.OrdinalIgnoreCase))
                collection.Add(brand);

            return collection.CreateIterator();
        }

        // Märken med sina modeller, bara de som finns för vald biltyp (null = alla)
        public List<CatalogBrand> GetCatalog(VehicleType? type = null)
        {
            lock (_lock)
            {
                var result = new List<CatalogBrand>();
                var iterator = CreateBrandIterator();

                while (iterator.HasNext())
                {
                    var brand = (VehicleBrand)iterator.Next();
                    var models = _models.GetByBrandId(brand.BrandId)
                        .Where(m => type is null || m.SupportedTypes.Contains(type.Value))
                        .Select(m => m.ModelName)
                        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (models.Count > 0)
                        result.Add(new CatalogBrand(brand.BrandName, models));
                }

                return result;
            }
        }

        public bool IsRegistered(string brand, string model, VehicleType type)
            => GetCatalog(type).Any(b =>
                string.Equals(b.Name, brand?.Trim(), StringComparison.OrdinalIgnoreCase)
                && b.Models.Any(m => string.Equals(m, model?.Trim(), StringComparison.OrdinalIgnoreCase)));

        // Lägger till en kombination. Nytt märke skapas vid behov. Kastar om kombinationen redan finns för typen.
        public void AddCombination(string brandName, string modelName, VehicleType type)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                throw new ArgumentException("Brand is required.");
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model is required.");

            lock (_lock)
            {
                if (!TryAdd(brandName, modelName, type))
                    throw new InvalidOperationException($"{brandName.Trim()} {modelName.Trim()} already exists for {type}.");
            }
        }

        // false om kombinationen redan fanns för typen
        private bool TryAdd(string brandName, string modelName, VehicleType type)
        {
            brandName = brandName.Trim();
            modelName = modelName.Trim();

            var brand = _brands.GetAll().FirstOrDefault(b => string.Equals(b.BrandName, brandName, StringComparison.OrdinalIgnoreCase));
            if (brand is null)
            {
                brand = new VehicleBrand(brandName, type);
                _brands.Add(brand);
            }
            else if (!brand.SupportedTypes.Contains(type))
            {
                brand.SupportedTypes.Add(type);
            }

            var model = _models.GetByBrandId(brand.BrandId)
                .FirstOrDefault(m => string.Equals(m.ModelName, modelName, StringComparison.OrdinalIgnoreCase));

            if (model is null)
            {
                model = new VehicleModel(modelName, brand.BrandId, type) { Brand = brand };
                _models.Add(model);
                brand.Models.Add(model);
                return true;
            }

            if (model.SupportedTypes.Contains(type))
                return false;

            model.SupportedTypes.Add(type);
            return true;
        }
    }
}
