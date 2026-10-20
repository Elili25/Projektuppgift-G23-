using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.VehicleNS
{
    public class VanFactory : IElectricVehicleFactory
    {
        private readonly IRegNrValidator _regNrValidator;
        private readonly IBrandModelValidator _brandModelValidator;
        private readonly IYearValidator _yearValidator;
        private readonly IHorsePowerValidator _horsePowerValidator;

        public VanFactory(
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
            double cargoCapacityM3,
            double maxPayloadKg,
            CountryCode countryCode)
        {
            // Validera indata
            _regNrValidator.ValidateRegNr(regNr);
            _brandModelValidator.ValidateBrand(brand);
            _brandModelValidator.ValidateModel(model);
            _yearValidator.ValidateYear(year);
            _horsePowerValidator.ValidateHorsePower(horsePower);

            // Skapa registreringsnummer som ett value object
            var registrationPlate = new RegistrationPlate(regNr, countryCode);

            // Skapa och returnera skåpbilen
            return new Van(
                registrationPlate,
                brand,
                model,
                year,
                horsePower,
                passengerCapacity,
                luggageCapacity,
                fuelType,
                transmissionType,
                cargoCapacityM3,
                maxPayloadKg);
        }

        public Vehicle CreateElectric(
            Vehicle baseVehicle,
            int batteryCapacityKwh,
            int rangeKm)
        {
            if (baseVehicle is not Van van)
            {
                throw new ArgumentException(
                    "BaseVehicle måste vara en Van",
                    nameof(baseVehicle));
            }

            return new ElectricVan(
                van,
                batteryCapacityKwh,
                rangeKm);
        }
    }
}