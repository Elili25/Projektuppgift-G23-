
using SeventWebApplication.Models.VehicleNS.Engine;

namespace SeventWebApplication.Models.VehicleNS
{
    public abstract class Vehicle
    {
        public Guid VehicleId { get; private set; } = Guid.NewGuid();

        public RegistrationPlate RegistrationPlate { get; private set; }

        public VehicleModel Model { get; private set; }

        public VehicleCategoryType VehicleCategoryType { get; private set; }

        public TransmissionType TransmissionType { get; set; }

        // Fordonet har en motor.
        public IEngine Engine { get; private set; }

        public int Year { get; private set; }
        public int PassengerCapacity { get; private set; }
        public decimal LuggageCapacity { get; private set; }

        // Bilens skick. Ändras bara via ChangeStatus, så att ändringen alltid går att spåra
        public VehicleStatus VehicleStatus { get; private set; } = VehicleStatus.Active;

        // Om bilen är uthyrd just nu (sätts vid pickup/retur)
        public bool IsAvailable { get; set; } = true;

        // Kan bilen köras överhuvudtaget? (inte trasig, på service eller avställd)
        public bool IsOperational => VehicleStatus == VehicleStatus.Active;

        //För viewmodels, för att kunna visa bild på bilen. Kan vara null om ingen bild finns.
        public string? ImagePath { get; set; }

        // Skickas när statusen ändras. Här kan VehicleStatusHistory koppla in sig senare.
        public event Action<Vehicle, VehicleStatus, VehicleStatus>? StatusChanged;

        protected Vehicle(
            RegistrationPlate registrationPlate,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            int year,
            int passengerCapacity,
            decimal luggageCapacity,
            IEngine engine,
            TransmissionType transmissionType)
        {      

            RegistrationPlate = registrationPlate
                ?? throw new ArgumentNullException(nameof(registrationPlate));

            Model = model
                ?? throw new ArgumentNullException(nameof(model));

            Engine = engine
                ?? throw new ArgumentNullException(nameof(engine));

            VehicleCategoryType = vehicleCategoryType;
            Year = year;
            PassengerCapacity = passengerCapacity;
            LuggageCapacity = luggageCapacity;
            TransmissionType = transmissionType;
        }
        // Redigering av beskrivande fält (regnummer, märke/modell, år, platser, bagage).
        // Valideringen görs av anroparen (factories/validators), motor och växellåda ändras inte.
        public void UpdateDetails(RegistrationPlate plate, VehicleModel model, int year, int passengerCapacity, decimal luggageCapacity)
        {
            RegistrationPlate = plate ?? throw new ArgumentNullException(nameof(plate));
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Year = year;
            PassengerCapacity = passengerCapacity;
            LuggageCapacity = luggageCapacity;
        }

        public void ChangeStatus(VehicleStatus newStatus)
        {
            if (newStatus == VehicleStatus)
                return;

            var oldStatus = VehicleStatus;
            VehicleStatus = newStatus;
            StatusChanged?.Invoke(this, oldStatus, newStatus);
        }
    }
}
