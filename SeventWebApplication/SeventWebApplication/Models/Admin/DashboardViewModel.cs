using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Pricing;
using SeventWebApplication.Models.Pricing.PricingCategories;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Admin
{
    // Allt Overview-sidan visar: nyckeltal, kommande bokningar och flottans fördelning.
    public class DashboardViewModel
    {
        // KPI-korten
        public int ActiveBookings { get; init; }
        public int PendingBookings { get; init; }
        public int AvailableVehicles { get; init; }
        public int TotalVehicles { get; init; }
        public int InServiceVehicles { get; init; }      // InService + InRepair + OutOfService
        public int InRepairVehicles { get; init; }
        public decimal RevenueThisMonth { get; init; }
        public int RevenueBookings { get; init; }

        // Flottans fördelning (munkdiagrammet)
        public int RentedVehicles { get; init; }
        public int AvailablePercent => Percent(AvailableVehicles);
        public int RentedPercent => Percent(RentedVehicles);
        public int ServicePercent => Percent(InServiceVehicles);

        // Tabellen med kommande bokningar
        public List<BookingRowViewModel> UpcomingBookings { get; init; } = new();

        private int Percent(int count) => TotalVehicles == 0 ? 0 : (int)Math.Round(count * 100.0 / TotalVehicles);

        public static DashboardViewModel Build(
            IEnumerable<Booking.Booking> bookings,
            IEnumerable<Vehicle> vehicles,
            IUserRepository users,
            PricingServices pricing)
        {
            var allBookings = bookings.ToList();
            var fleet = vehicles.ToList();
            DateTime now = DateTime.Now;

            // Flottan: ledig = aktiv och inte uthyrd, uthyrd = aktiv men utlånad, resten är ur drift
            int available = fleet.Count(v => v.VehicleStatus == VehicleStatus.Active && v.IsAvailable);
            int rented = fleet.Count(v => v.VehicleStatus == VehicleStatus.Active && !v.IsAvailable);
            int outOfOrder = fleet.Count - available - rented;

            // Intäkt denna månad: bokningar som startar i månaden och är godkända (inte väntande/avbokade).
            // Priset räknas av PricingServices, samma som bokningsformuläret.
            decimal revenue = 0;
            int revenueBookings = 0;
            foreach (var b in allBookings.Where(b =>
                b.Status is BookingStatus.Confirmed or BookingStatus.Active or BookingStatus.Completed
                && b.StartDate.Year == now.Year && b.StartDate.Month == now.Month))
            {
                if (b.Vehicle is null) continue;

                try
                {
                    int days = Math.Max(1, (int)Math.Ceiling((b.EndDate - b.StartDate).TotalDays));
                    revenue += pricing.CalculateTotalPrice(PriceStrategies.standard, b, b.Vehicle, days);
                    revenueBookings++;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Fordon utan prissättning hoppas över
                }
            }

            var upcoming = allBookings
                .Where(b => b.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.Active
                         && b.EndDate >= now)
                .OrderBy(b => b.StartDate)
                .Take(6)
                .Select(b => BookingRowViewModel.From(b, users, now))
                .ToList();

            return new DashboardViewModel
            {
                ActiveBookings = allBookings.Count(b => b.Status == BookingStatus.Active),
                PendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending),
                AvailableVehicles = available,
                RentedVehicles = rented,
                InServiceVehicles = outOfOrder,
                InRepairVehicles = fleet.Count(v => v.VehicleStatus == VehicleStatus.InRepair),
                TotalVehicles = fleet.Count,
                RevenueThisMonth = revenue,
                RevenueBookings = revenueBookings,
                UpcomingBookings = upcoming
            };
        }
    }
}
