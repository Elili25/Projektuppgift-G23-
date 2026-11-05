using SeventWebApplication.Models.VehicleNS.Engine;

namespace SeventWebApplication.Models.VehicleNS
{
    // Factory Method: skapar personbilar och validerar indata innan bilen byggs.
    // Create(request) är den gemensamma vägen in (IVehicleFactory).
    // CreateStandard/CreateElectric finns kvar för kod som anropar dem direkt.
    public class CarFactory : IVehicleFactory<CarCreateRequest>
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

        // --- Gemensam väg in (IVehicleFactory) ---

        // Väljer rätt metod utifrån bränsletyp. Kategorin sätts alltid till PassengerCar,
        // så en Car kan aldrig råka få fel VehicleCategoryType.
        public Vehicle Create(CarCreateRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.FuelType == FuelType.Electric)
            {
                return CreateElectric(
                    request.RegNr,
                    request.Model,
                    VehicleCategoryType.PassengerCar,
                    request.CategoryType,
                    request.ComfortType,
                    request.Year,
                    request.HorsePower,
                    request.PassengerCapacity,
                    request.LuggageCapacity,
                    request.BatteryCapacityKwh,
                    request.RangeKm,
                    request.TransmissionType,
                    request.CountryCode);
            }

            return CreateStandard(
                request.RegNr,
                request.Model,
                VehicleCategoryType.PassengerCar,
                request.CategoryType,
                request.ComfortType,
                request.Year,
                request.HorsePower,
                request.PassengerCapacity,
                request.LuggageCapacity,
                request.FuelType,
                request.TransmissionType,
                request.CountryCode,
                request.FuelCapacityLiters);
        }

        // --- Bensin och diesel ---

        public Vehicle CreateStandard(
            string regNr,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            PassengerCarCategoryType passengerCarCategoryType,
            VehicleComfortType vehicleComfortType,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            FuelType fuelType,
            TransmissionType transmissionType,
            CountryCode countryCode,
            int fuelCapacityLiters = 50)
        {
            ValidateCommonFields(regNr, model, year, horsePower);

            if (fuelCapacityLiters <= 0)
                throw new ArgumentOutOfRangeException(nameof(fuelCapacityLiters));

            IEngine engine = fuelType switch
            {
                FuelType.Petrol => new PetrolEngine(horsePower, fuelCapacityLiters),
                FuelType.Diesel => new DieselEngine(horsePower, fuelCapacityLiters),
                _ => throw new ArgumentException(
                    "Använd CreateElectric för elbilar. Hybrid stöds inte än.",
                    nameof(fuelType))
            };

            return BuildCar(
                regNr, countryCode, model, vehicleCategoryType,
                passengerCarCategoryType, vehicleComfortType,
                year, passengerCapacity, luggageCapacity,
                engine, transmissionType);
        }

        // --- El ---

        public Vehicle CreateElectric(
            string regNr,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            PassengerCarCategoryType passengerCarCategoryType,
            VehicleComfortType vehicleComfortType,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            int batteryCapacityKwh,
            int rangeKm,
            TransmissionType transmissionType,
            CountryCode countryCode)
        {
            ValidateCommonFields(regNr, model, year, horsePower);

            if (batteryCapacityKwh <= 0)
                throw new ArgumentOutOfRangeException(nameof(batteryCapacityKwh));

            if (rangeKm <= 0)
                throw new ArgumentOutOfRangeException(nameof(rangeKm));

            IEngine engine = new ElectricEngine(horsePower, batteryCapacityKwh, rangeKm);

            return BuildCar(
                regNr, countryCode, model, vehicleCategoryType,
                passengerCarCategoryType, vehicleComfortType,
                year, passengerCapacity, luggageCapacity,
                engine, transmissionType);
        }

        // --- Hjälpmetoder ---

        // Skapar själva bilen, gemensamt för alla motortyper
        private static Car BuildCar(
            string regNr,
            CountryCode countryCode,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            PassengerCarCategoryType passengerCarCategoryType,
            VehicleComfortType vehicleComfortType,
            int year,
            int passengerCapacity,
            decimal luggageCapacity,
            IEngine engine,
            TransmissionType transmissionType)
        {
            return new Car(
                new RegistrationPlate(regNr, countryCode),
                model,
                vehicleCategoryType,
                passengerCarCategoryType,
                vehicleComfortType,
                year,
                passengerCapacity,
                luggageCapacity,
                engine,
                transmissionType);
        }

        private void ValidateCommonFields(
            string regNr,
            VehicleModel model,
            int year,
            int horsePower)
        {
            _regNrValidator.ValidateRegNr(regNr);

            if (model?.Brand == null)
                throw new ArgumentException(
                    "Modellen måste ha ett kopplat bilmärke.",
                    nameof(model));

            _brandModelValidator.ValidateBrand(model.Brand.BrandName);
            _brandModelValidator.ValidateModel(model.ModelName);
            _yearValidator.ValidateYear(year);
            _horsePowerValidator.ValidateHorsePower(horsePower);
        }
    }
}