using SeventWebApplication.Models.VehicleNS.Engine;

namespace SeventWebApplication.Models.VehicleNS
{
    // Factory Method: skapar skåpbilar och validerar indata innan bilen byggs.
    // Create(request) är den gemensamma vägen in (IVehicleFactory).
    // CreateStandard/CreateElectric finns kvar för kod som anropar dem direkt.
    public class VanFactory : IVehicleFactory<VanCreateRequest>
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

        // --- Gemensam väg in (IVehicleFactory) ---

        // Väljer rätt metod utifrån bränsletyp. Kategorin sätts alltid till Van,
        // så en Van kan aldrig råka få fel VehicleCategoryType.
        public Vehicle Create(VanCreateRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.FuelType == FuelType.Electric)
            {
                return CreateElectric(
                    request.RegNr,
                    request.Model,
                    VehicleCategoryType.Van,
                    request.CategoryType,
                    request.Year,
                    request.HorsePower,
                    request.PassengerCapacity,
                    request.LuggageCapacity,
                    request.BatteryCapacityKwh,
                    request.RangeKm,
                    request.TransmissionType,
                    request.CargoCapacityM3,
                    request.MaxPayloadKg,
                    request.CountryCode);
            }

            return CreateStandard(
                request.RegNr,
                request.Model,
                VehicleCategoryType.Van,
                request.CategoryType,
                request.Year,
                request.HorsePower,
                request.PassengerCapacity,
                request.LuggageCapacity,
                request.FuelType,
                request.TransmissionType,
                request.CargoCapacityM3,
                request.MaxPayloadKg,
                request.CountryCode,
                request.FuelCapacityLiters);
        }

        // --- Bensin och diesel ---

        public Vehicle CreateStandard(
            string regNr,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            VanCategoryType vanCategoryType,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            FuelType fuelType,
            TransmissionType transmissionType,
            double cargoCapacityM3,
            double maxPayloadKg,
            CountryCode countryCode,
            int fuelCapacityLiters = 50)
        {
            ValidateCommonFields(regNr, model, year, horsePower);
            ValidateCargo(cargoCapacityM3, maxPayloadKg);

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

            return BuildVan(
                regNr, countryCode, model, vehicleCategoryType, vanCategoryType,
                year, passengerCapacity, luggageCapacity,
                engine, transmissionType, cargoCapacityM3, maxPayloadKg);
        }

        // --- El ---

        public Vehicle CreateElectric(
            string regNr,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            VanCategoryType vanCategoryType,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            int batteryCapacityKwh,
            int rangeKm,
            TransmissionType transmissionType,
            double cargoCapacityM3,
            double maxPayloadKg,
            CountryCode countryCode)
        {
            ValidateCommonFields(regNr, model, year, horsePower);
            ValidateCargo(cargoCapacityM3, maxPayloadKg);

            if (batteryCapacityKwh <= 0)
                throw new ArgumentOutOfRangeException(nameof(batteryCapacityKwh));

            if (rangeKm <= 0)
                throw new ArgumentOutOfRangeException(nameof(rangeKm));

            IEngine engine = new ElectricEngine(horsePower, batteryCapacityKwh, rangeKm);

            return BuildVan(
                regNr, countryCode, model, vehicleCategoryType, vanCategoryType,
                year, passengerCapacity, luggageCapacity,
                engine, transmissionType, cargoCapacityM3, maxPayloadKg);
        }

        // --- Hjälpmetoder ---

        // Skapar själva skåpbilen, gemensamt för alla motortyper
        private static Van BuildVan(
            string regNr,
            CountryCode countryCode,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            VanCategoryType vanCategoryType,
            int year,
            int passengerCapacity,
            decimal luggageCapacity,
            IEngine engine,
            TransmissionType transmissionType,
            double cargoCapacityM3,
            double maxPayloadKg)
        {
            return new Van(
                new RegistrationPlate(regNr, countryCode),
                model,
                vehicleCategoryType,
                vanCategoryType,
                year,
                passengerCapacity,
                luggageCapacity,
                engine,
                transmissionType,
                cargoCapacityM3,
                maxPayloadKg);
        }

        private static void ValidateCargo(double cargoCapacityM3, double maxPayloadKg)
        {
            if (cargoCapacityM3 <= 0)
                throw new ArgumentOutOfRangeException(nameof(cargoCapacityM3), "Cargo capacity must be greater than 0.");

            if (maxPayloadKg <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPayloadKg), "Max payload must be greater than 0.");
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