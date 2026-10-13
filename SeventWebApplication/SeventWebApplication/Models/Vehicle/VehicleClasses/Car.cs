namespace SeventWebApplication.Models.Vehicle
{
    public class Car : Vehicle, IPassengerVehicle, IVehicleWithEngine, IVehicleTransmissionType
    {
        public int PassengerCapacity { get; set; }
        public FuelType FuelType { get; set; }
        public TransmissionType TransmissionType { get; set; } 
        public Car
            (
            string regNr, 
            string brand, 
            string model, 
            int year, 
            int horsePower,         
            int passengerCapacity, 
            FuelType fuelType,
            TransmissionType transmissionType
            )
            :base(regNr, brand, model, year, horsePower)

        {
            PassengerCapacity = passengerCapacity;
            FuelType = fuelType;                          
            TransmissionType = transmissionType;           
        }

        //fixa här nedan senare detta är placeholder för nu
        public override decimal GetBasePrice() => 500m;

        public override string GetDescription() =>
            $"{Brand} {Model} ({Year}) - {FuelType} - {TransmissionType} - {PassengerCapacity} seats";
    }
}
