namespace SeventWebApplication.Models.VehicleNS
{
    public class BrandModelValidator : IBrandModelValidator
    {
        public void ValidateBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Brand cannot be empty");
        }

        public void ValidateModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be empty");
        }
    }
}
