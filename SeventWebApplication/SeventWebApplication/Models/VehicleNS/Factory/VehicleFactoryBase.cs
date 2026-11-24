namespace SeventWebApplication.Models.VehicleNS
{
    // Gemensam bas för CarFactory och VanFactory. Valideringen av regnummer, märke/modell, år och hästkrafter
    // är densamma för alla fordonstyper, så den ligger här en gång i stället för att kopieras i varje fabrik.
    // Fabrikerna ärver bara det de delar. Det som skiljer (skåpbilens lastvolym, bilens komfort) ligger kvar i respektive klass.
    public abstract class VehicleFactoryBase
    {
        private readonly IRegNrValidator _regNrValidator;
        private readonly IBrandModelValidator _brandModelValidator;
        private readonly IYearValidator _yearValidator;
        private readonly IHorsePowerValidator _horsePowerValidator;

        protected VehicleFactoryBase(
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

        protected void ValidateCommonFields(
            string regNr,
            VehicleModel model,
            int year,
            int horsePower)
        {
            _regNrValidator.ValidateRegNr(regNr);

            if (model?.Brand == null)
                throw new ArgumentException(
                    "The model must have a brand.",
                    nameof(model));

            _brandModelValidator.ValidateBrand(model.Brand.BrandName);
            _brandModelValidator.ValidateModel(model.ModelName);
            _yearValidator.ValidateYear(year);
            _horsePowerValidator.ValidateHorsePower(horsePower);
        }
    }
}
