using SeventWebApplication.Models.Maintenance.Services;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Maintenance.Seed
{
    // Seeder för två demobilar som visar skador respektive vanligt underhåll i admin.
    // Bilarna har inga bokningar (ExampleBookingSeeder slumpar bara bland sina egna bilar).
    //
    // Båda är SUV + Economy + Manual, som ingen av de seedade familjerna matchar. Då kopplas de inte till
    // någon familj, och VehicleAutoAssignWorker kan inte tilldela dem till en familjebokning senare.
    //
    // Körs efter att familjerna är seedade i Program.cs, på samma sätt som ExampleBookingSeeder.
    public static class ExampleMaintenanceSeeder
    {
        public const string DamagedRegNr = "DAM421";
        public const string MaintenanceRegNr = "SER318";

        public static void Seed(VehicleDamageService damages, VehicleMaintenanceService maintenance)
        {
            // Redan seedat (eller en bil med samma regnummer finns), hoppa över
            var fleet = FleetManager.instance;
            if (fleet.GetAllVehicles().Any(v => v.RegistrationPlate.RegNr is DamagedRegNr or MaintenanceRegNr))
                return;

            SeedDamagedVehicle(fleet, damages);
            SeedMaintenanceVehicle(fleet, maintenance);
        }

        // Bil med två öppna skador. RegisterDamage sätter bilens status till InRepair,
        // så den syns under Fleet > Service & repairs och Returns > Damages.
        private static void SeedDamagedVehicle(FleetManager fleet, VehicleDamageService damages)
        {
            var car = ExampleBookingSeeder.MakeCar(
                "Skoda", "Kodiaq", DamagedRegNr,
                PassengerCarCategoryType.SUV, VehicleComfortType.Economy, TransmissionType.Manual,
                2021, 150, FuelType.Diesel, 560m);

            fleet.AddVehicle(car);

            damages.RegisterDamage(new VehicleDamage
            {
                VehicleId = car.VehicleId,
                Description = "Dent and scratches on the rear bumper.",
                ReportedAt = DateTime.UtcNow.AddDays(-3)
            });

            damages.RegisterDamage(new VehicleDamage
            {
                VehicleId = car.VehicleId,
                Description = "Cracked tail light, left side.",
                ReportedAt = DateTime.UtcNow.AddDays(-3)
            });
        }

        // Bil som bara behöver vanligt underhåll: en avslutad oljebyte i historiken
        // och en pågående service som är planerad att vara klar om två dagar.
        private static void SeedMaintenanceVehicle(FleetManager fleet, VehicleMaintenanceService maintenance)
        {
            var car = ExampleBookingSeeder.MakeCar(
                "Kia", "Sportage", MaintenanceRegNr,
                PassengerCarCategoryType.SUV, VehicleComfortType.Economy, TransmissionType.Manual,
                2022, 150, FuelType.Petrol, 540m);

            fleet.AddVehicle(car);

            var today = DateTime.UtcNow.Date;

            // Historik: avslutat oljebyte för tre månader sedan
            maintenance.RegisterMaintenance(
                car.VehicleId, MaintenanceType.Oilchange, "Oil and filter change.",
                today.AddDays(-90), today.AddDays(-90),
                mileage: 28_400, cost: 1_295m, notes: "Done at the regular service interval.");

            // Pågående: årsservice, började igår och väntas vara klar om två dagar
            maintenance.ScheduleMaintenance(
                car.VehicleId, MaintenanceType.Service, "Annual service.",
                today.AddDays(-1), today.AddDays(2),
                mileage: 41_250, notes: "Brake check and inspection included.");
        }
    }
}
