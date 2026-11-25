using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Admin
{
    // En bokning som en stapel i tidslinjen
    public class TimelineBar
    {
        public string Label { get; init; } = "";
        public string Tooltip { get; init; } = "";
        public BookingStatus Status { get; init; }
        public List<string> Tags { get; init; } = new();   // t.ex. SUV, Automatic, Premium
        public VehicleStatus? VehicleStatus { get; init; } // null = ingen bil tilldelad än
        public int ColStart { get; init; }          // 1 = första dagen i intervallet
        public int Span { get; init; }
        public bool ContinuesBefore { get; init; }  // börjar före intervallet
        public bool ContinuesAfter { get; init; }   // slutar efter intervallet
    }

    // En rad: ett fordon (eller en rubrikrad för Car/Van, eller en bokning utan bil än)
    public class TimelineRow
    {
        public string Label { get; init; } = "";
        public string Sub { get; init; } = "";
        public bool IsGroup { get; init; }
        public int GroupLevel { get; init; } = 1;   // 1 = Passenger Cars / Vans, 2 = kategori (SUV, Cargo van ...)
        public List<TimelineBar> Bars { get; init; } = new();
    }

    // Tidslinjen på /Calendar: fordon som rader, dagar som kolumner, bokningar som staplar.
    public class TimelineViewModel
    {
        public string View { get; init; } = "month";   // week | month | 2months
        public DateTime Start { get; init; }
        public int Days { get; init; }
        public List<TimelineRow> Rows { get; init; } = new();

        public DateTime End => Start.AddDays(Days - 1);
        public DateTime Previous { get; init; }
        public DateTime Next { get; init; }

        public string Title
        {
            get
            {
                var en = System.Globalization.CultureInfo.GetCultureInfo("en-US");
                return $"{Start.ToString("d MMM", en)} – {End.ToString("d MMM yyyy", en)}";
            }
        }

        public static TimelineViewModel Build(
            IEnumerable<Booking.Booking> bookings,
            IEnumerable<Vehicle> vehicles,
            IUserRepository users,
            VehicleFamilyRepository families,
            string? view,
            DateTime anchor)
        {
            view = view is "week" or "2months" ? view : "month";

            DateTime start;
            int days;
            DateTime previous, next;

            // Rullande intervall som börjar på valt datum (som standard idag): 14, 30 eller 60 dagar framåt.
            // Pilarna flyttar ett helt intervall bakåt/framåt.
            start = anchor.Date;
            days = view == "week" ? 14 : view == "2months" ? 60 : 30;
            previous = start.AddDays(-days);
            next = start.AddDays(days);

            DateTime last = start.AddDays(days - 1);
            DateTime now = DateTime.Now;

            var inRange = bookings
                .Where(b => b.Status != BookingStatus.Cancelled
                         && b.StartDate.Date <= last && b.EndDate.Date >= start)
                .OrderBy(b => b.StartDate)
                .ToList();

            // Bara för- och efternamn, inga mellannamn
            static string ShortName(string full)
            {
                var parts = full.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length <= 2 ? string.Join(' ', parts) : $"{parts[0]} {parts[^1]}";
            }

            // Kategori, växellåda och komfort för bilen, eller för familjen om ingen bil är vald än
            List<string> TagsFor(Booking.Booking b)
            {
                if (b.Vehicle is Car car)
                    return new() { car.PassengerCarCategoryType.ToString(), car.TransmissionType.ToString(), car.VehicleComfortType.ToString() };
                if (b.Vehicle is Van van)
                    return new() { van.VanCategoryType.ToString(), van.TransmissionType.ToString() };

                return b.VehicleFamilyId is Guid id ? families.GetById(id) switch
                {
                    PassengerCarFamily f => new() { f.CategoryType.ToString(), f.TransmissionType.ToString(), f.ComfortType.ToString() },
                    VanFamily f => new() { f.CategoryType.ToString(), f.TransmissionType.ToString() },
                    _ => new()
                } : new();
            }

            TimelineBar ToBar(Booking.Booking b)
            {
                var row = BookingRowViewModel.From(b, users, now);
                int colStart = Math.Max(0, (b.StartDate.Date - start).Days) + 1;
                int colEnd = Math.Min(days - 1, (b.EndDate.Date - start).Days) + 1;

                return new TimelineBar
                {
                    Label = ShortName(row.CustomerName),
                    Tooltip = $"#{row.ShortId} {row.CustomerName} – {row.Vehicle} {row.RegNr} ({b.StartDate:d MMM HH:mm} → {b.EndDate:d MMM HH:mm}), booking {b.Status}"
                              + (b.Vehicle is null ? "" : $", vehicle {b.Vehicle.VehicleStatus}"),
                    Status = b.Status,
                    Tags = TagsFor(b),
                    VehicleStatus = b.Vehicle?.VehicleStatus,
                    ColStart = colStart,
                    Span = Math.Max(1, colEnd - colStart + 1),
                    ContinuesBefore = b.StartDate.Date < start,
                    ContinuesAfter = b.EndDate.Date > last
                };
            }

            var rows = new List<TimelineRow>();

            // Fordonen i två nivåer: Passenger Cars / Vans, och under dem kategorierna (SUV, Sedan ... Cargo van, Passenger van).
            static (int Order, string Name) CategoryOf(Vehicle v) => v switch
            {
                Car c => ((int)c.PassengerCarCategoryType, c.PassengerCarCategoryType switch
                {
                    PassengerCarCategoryType.StationWagon => "Station wagon",
                    var t => t.ToString()
                }),
                Van van => ((int)van.VanCategoryType, van.VanCategoryType switch
                {
                    VanCategoryType.CargoVan => "Cargo van",
                    _ => "Passenger van"
                }),
                _ => (999, "Other")
            };

            foreach (var top in vehicles
                .GroupBy(v => v is Van ? "Vans" : "Passenger Cars")
                .OrderBy(g => g.Key == "Vans" ? 1 : 0))
            {
                rows.Add(new TimelineRow { Label = top.Key, IsGroup = true, GroupLevel = 1 });

                foreach (var category in top.GroupBy(CategoryOf).OrderBy(g => g.Key.Order))
                {
                    rows.Add(new TimelineRow { Label = category.Key.Name, IsGroup = true, GroupLevel = 2 });

                    foreach (var v in category.OrderBy(v => v.RegistrationPlate.RegNr))
                    {
                        rows.Add(new TimelineRow
                        {
                            Label = $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                            Sub = v.RegistrationPlate.RegNr,
                            Bars = inRange.Where(b => b.VehicleId == v.VehicleId).Select(ToBar).ToList()
                        });
                    }
                }
            }

            // Bokningar av en hel familj som inte har fått en bil än, en rad per bokning så de inte hamnar ovanpå varandra
            var unassigned = inRange.Where(b => b.VehicleId is null).ToList();
            if (unassigned.Count > 0)
            {
                rows.Add(new TimelineRow { Label = "Waiting for a vehicle", IsGroup = true });

                foreach (var b in unassigned)
                {
                    string family = b.VehicleFamilyId is Guid id ? families.GetById(id)?.Name ?? "Family" : "–";
                    rows.Add(new TimelineRow { Label = family, Sub = "no vehicle yet", Bars = { ToBar(b) } });
                }
            }

            return new TimelineViewModel
            {
                View = view,
                Start = start,
                Days = days,
                Rows = rows,
                Previous = previous,
                Next = next
            };
        }
    }
}
