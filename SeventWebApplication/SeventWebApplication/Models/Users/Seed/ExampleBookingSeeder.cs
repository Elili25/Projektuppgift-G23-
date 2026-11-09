using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.VehicleNS;
using SeventWebApplication.Models.VehicleNS.Engine;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;
using BookingStatus = SeventWebApplication.Models.Booking.BookingStatus;

namespace SeventWebApplication.Models.Users
{
    // Seeder som skapar exempelbilar och exempelbokningar för att kunna testa appen.
    //
    // Bilarna läggs till i flottan via FleetManager.AddVehicle. VehicleAssignmentService lyssnar på
    // VehicleAdded och kopplar då varje bil till rätt familj automatiskt.
    // Måste därför köras EFTER att familjerna har seedats i Program.cs.
    public static class ExampleBookingSeeder
    {
        public static void Seed(IUserRepository users)
        {
            var registry = SeventWebApplication.Models.Booking.BookingRegistry.Instance;

            if (registry.GetBookings().Any())
                return;

            var allUsers = users.GetAll().ToList();
            if (allUsers.Count == 0)
                return;

            var random = new Random();

            // Varje bil matchar en familj i VehicleFamilySeeder:
            //   Luxury Sedan       = Sedan + Luxury + Automatic
            //   Premium SUV        = SUV + Premium + Automatic
            //   Standard Hatchback = Hatchback + Standard + Manual
            //   Cargo Van          = CargoVan + Manual, minst 8 m³ och 500 kg
            // (Economy Station Wagon fylls av FleetManager.SeedTestVehicle, regnr ABC123)
            var vehicles = new List<Vehicle>
            {
                MakeCar("Tesla", "Model 3", "DEF456", PassengerCarCategoryType.Sedan, VehicleComfortType.Luxury, TransmissionType.Automatic, 2023, 283, FuelType.Electric, 425m),
                MakeCar("BMW", "i4", "BMW999", PassengerCarCategoryType.Sedan, VehicleComfortType.Luxury, TransmissionType.Automatic, 2022, 340, FuelType.Electric, 470m),
                MakeCar("Volvo", "XC60", "PRE123", PassengerCarCategoryType.SUV, VehicleComfortType.Premium, TransmissionType.Automatic, 2024, 250, FuelType.Petrol, 483m),
                MakeCar("Volkswagen", "Golf", "GLF777", PassengerCarCategoryType.Hatchback, VehicleComfortType.Standard, TransmissionType.Manual, 2022, 150, FuelType.Diesel, 380m),
                MakeVan("Volkswagen", "Transporter", "VAN001", VanCategoryType.CargoVan, TransmissionType.Manual, 2021, 150, 10.0, 900.0)
            };

            foreach (var vehicle in vehicles)
                FleetManager.instance.AddVehicle(vehicle);

            // Bara statusar som går att ha utan att pickup/retur faktiskt har körts.
            // Active och Completed skapas via BookingManagementService.StartRental/CompleteRental,
            // annars saknas rental record och inspektion för bokningen.
            var statuses = new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Cancelled };

            // Bokningar på en specifik bil
            for (int i = 0; i < 10; i++)
            {
                var user = allUsers[random.Next(allUsers.Count)];
                var vehicle = vehicles[random.Next(vehicles.Count)];

                var start = DateTime.Today.AddDays(random.Next(1, 30));
                var end = start.AddDays(random.Next(1, 7));

                TryRegister(registry, new BookingEntity(
                    Guid.NewGuid().ToString(),
                    user.UserId.ToString(),
                    vehicle,
                    start,
                    end,
                    RandomAddons(random))
                {
                    Status = statuses[random.Next(statuses.Length)]
                });
            }

            // Familjebokningar utan bil, så att Approve (som tilldelar en bil ur familjen) går att testa
            var familyIds = new[]
            {
                VehicleFamilySeeder.LuxurySedanId,
                VehicleFamilySeeder.PremiumSuvId,
                VehicleFamilySeeder.StandardHatchbackId
            };

            foreach (var familyId in familyIds)
            {
                var user = allUsers[random.Next(allUsers.Count)];
                var start = DateTime.Today.AddDays(random.Next(30, 45));

                TryRegister(registry, new BookingEntity(
                    Guid.NewGuid().ToString(),
                    user.UserId.ToString(),
                    familyId,
                    start,
                    start.AddDays(3),
                    RandomAddons(random)));   // Status = Pending (standard)
            }
        }

        // Slumpen kan välja en bil som redan är bokad den perioden. Hoppa över i så fall,
        // annars kraschar appen vid start ibland.
        private static void TryRegister(
            SeventWebApplication.Models.Booking.BookingRegistry registry,
            BookingEntity booking)
        {
            try
            {
                registry.RegisterBooking(booking);
            }
            catch (InvalidOperationException)
            {
            }
        }

        // Slumpar fram tillval och samlar dem i ett AddonPackage.
        // AddonPackage slår själv ihop samma typ av tillval till en rad med ökat antal.
        // Max 3 barnstolar, samma gräns som i AddonValidationStrategy.
        private static AddonPackage RandomAddons(Random random)
        {
            var package = new AddonPackage();

            if (random.Next(2) == 0)                 // GPS i ungefär hälften av bokningarna
                package.Add(new GPSAddon());

            int childSeats = random.Next(0, 4);      // 0–3 barnstolar
            if (childSeats > 0)
                package.Add(new ChildSeatAddon { Quantity = childSeats });

            if (random.Next(3) == 0)                 // extra förare i ungefär var tredje
                package.Add(new ExtraDriverAddon());

            return package;
        }

        private static Car MakeCar(
            string brandName,
            string modelName,
            string regNr,
            PassengerCarCategoryType categoryType,
            VehicleComfortType comfortType,
            TransmissionType transmissionType,
            int year,
            int horsePower,
            FuelType fuel,
            decimal luggage)
        {
            return new Car(
                new RegistrationPlate(regNr, CountryCode.SE),
                MakeModel(brandName, modelName),
                VehicleCategoryType.PassengerCar,
                categoryType,
                comfortType,
                year,
                5,
                luggage,
                MakeEngine(fuel, horsePower),
                transmissionType);
        }

        private static Van MakeVan(
            string brandName,
            string modelName,
            string regNr,
            VanCategoryType categoryType,
            TransmissionType transmissionType,
            int year,
            int horsePower,
            double cargoCapacityM3,
            double maxPayloadKg)
        {
            return new Van(
                new RegistrationPlate(regNr, CountryCode.SE),
                MakeModel(brandName, modelName),
                VehicleCategoryType.Van,
                categoryType,
                year,
                3,
                0m,
                MakeEngine(FuelType.Diesel, horsePower),
                transmissionType,
                cargoCapacityM3,
                maxPayloadKg);
        }

        private static VehicleModel MakeModel(string brandName, string modelName)
        {
            var brand = new VehicleBrand(brandName);
            return new VehicleModel(modelName, brand.BrandId) { Brand = brand };
        }

        private static IEngine MakeEngine(FuelType fuel, int horsePower)
        {
            return fuel switch
            {
                FuelType.Petrol => new PetrolEngine(horsePower, 50),
                FuelType.Diesel => new DieselEngine(horsePower, 70),
                FuelType.Electric => new ElectricEngine(horsePower, 75, 450),
                _ => throw new ArgumentOutOfRangeException(nameof(fuel), fuel, "Unsupported fuel type.")
            };
        }
    }
}