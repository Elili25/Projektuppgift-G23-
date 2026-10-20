using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.VehicleNS
{
    public class CarFactory : IElectricVehicleFactory
    {
        private readonly IRegNrValidator _regNrValidator;
        private readonly IBrandModelValidator _brandModelValidator;
        private readonly IYearValidator _yearValidator;
        private readonly IHorsePowerValidator _horsePowerValidator;

        public CarFactory(
            IRegNrValidator regNrValidator,
            IBrandModelValidator brandModelValidator,
            IYearValidator yearValidator,
            IHorsePowerValidator horsePowerValidator)
        {
            _regNrValidator = regNrValidator;
            _brandModelValidator = brandModelValidator;
            _yearValidator = yearValidator;
            _horsePowerValidator = horsePowerValidator;
        }

        public Vehicle CreateStandard(
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            FuelType fuelType,
            TransmissionType transmissionType,
            CountryCode countryCode)
        {
            _regNrValidator.ValidateRegNr(regNr);
            _brandModelValidator.ValidateBrand(brand);
            _brandModelValidator.ValidateModel(model);
            _yearValidator.ValidateYear(year);
            _horsePowerValidator.ValidateHorsePower(horsePower);

            return new Car(regNr, brand, model, year, horsePower,
                passengerCapacity, luggageCapacity, fuelType, transmissionType, countryCode);
        }

        public Vehicle CreateElectric(Vehicle baseVehicle, int batteryCapacityKwh, int rangeKm)
        {
            if (baseVehicle is not Car car)
                throw new ArgumentException("BaseVehicle måste vara en Car", nameof(baseVehicle));

            return new ElectricCar(car, batteryCapacityKwh, rangeKm);
        }
    }
}
