namespace SeventWebApplication.Models.VehicleNS
{
    public interface IBrandModelValidator
    {
        void ValidateBrand(string brand);
        void ValidateModel(string model);
    }
}
