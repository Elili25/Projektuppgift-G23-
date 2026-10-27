using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.VehicleNS;
using BookingModel = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Users
{
    // vibe code seeder som skapar några exempelbokningar för att kunna testa bokningsvisaren.
    public static class ExampleBookingSeeder
    {
        public static void Seed(IUserRepository users)
        {
            var fleet = global::FleetManager.instance;

            if (fleet.GetBookings().Any())
                return;

            var allUsers = users.GetAll().ToList();
            if (allUsers.Count == 0)
                return;

            var random = new Random();
            var cars = new[]
            {
                MakeCar("Volvo", "V60", "ABC123", VehicleCategoryType.SUV, 2024, 197, FuelType.Petrol, 529),
                MakeCar("Tesla", "Model 3", "DEF456", VehicleCategoryType.Sedan, 2023, 283, FuelType.Electric, 425),
                MakeCar("Volkswagen", "Golf", "GLF777", VehicleCategoryType.Hatchback, 2022, 150, FuelType.Diesel, 380),
                MakeCar("BMW", "i3", "BMW999", VehicleCategoryType.Sedan, 2022, 250, FuelType.Electric, 300)
            };

            var statuses = new[] { "Pending", "Confirmed", "Active", "Completed", "Cancelled" };

            for (int i = 0; i < 10; i++)
            {
                var user = allUsers[random.Next(allUsers.Count)];
                var car = cars[random.Next(cars.Length)];
                var status = statuses[random.Next(statuses.Length)];

                var start = DateTime.Today.AddDays(random.Next(-20, 25));
                var end = start.AddDays(random.Next(1, 7));

                fleet.RegisterBooking(new BookingModel(
                    Guid.NewGuid().ToString(),
                    user.UserId.ToString(),
                    car,
                    start,
                    end,
                    RandomAddons(random, new VehicleBase(500m + i * 75m)))
                {
                    Status = status
                });
            }
        }

        // Slumpar fram tillval och lindar dem runt grundpriset (Decorator).
        // T.ex. kan det bli ChildSeatAddon(GPSAddon(VehicleBase)), och GetPrice() räknar ihop allt.
        // Max 3 barnstolar, samma gräns som i AddonValidationStrategy.
        private static IAddon RandomAddons(Random random, IAddon price)
        {
            if (random.Next(2) == 0)                 // GPS i ungefär hälften av bokningarna
                price = new GPSAddon(price);

            int childSeats = random.Next(0, 4);      // 0–3 barnstolar
            for (int i = 0; i < childSeats; i++)
                price = new ChildSeatAddon(price);

            if (random.Next(3) == 0)                 // extra förare i ungefär var tredje
                price = new ExtraDriverAddon(price);

            return price;
        }

        private static Car MakeCar(string brandName, string modelName, string regNr,
                                   VehicleCategoryType category, int year, int horsePower,
                                   FuelType fuel, decimal luggage)
        {
            var brand = new VehicleBrand(brandName);
            var model = new VehicleModel(modelName, brand.BrandId) { Brand = brand };

            return new Car(
                new RegistrationPlate(regNr, CountryCode.SE),
                model,
                category,
                year,
                horsePower,
                5,
                luggage,
                fuel,
                TransmissionType.Automatic);
        }
    }
}
