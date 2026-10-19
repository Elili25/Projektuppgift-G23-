namespace SeventWebApplication.Models.VehicleNS
{
    public class RegNrValidatorAdapter : IRegNrValidator
    {

        private readonly IRegNrValidator _strategy;

        public RegNrValidatorAdapter(IRegNrValidator strategy)
        {
            _strategy = strategy;
        }

        public void ValidateRegNr(string regNr) => _strategy.ValidateRegNr(regNr);
    }
}
