using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Models.Admin
{
    // En bokning på en dag i kalendern
    public class CalendarEntry
    {
        public string ShortId { get; init; } = "";
        public string Customer { get; init; } = "";
        public string Vehicle { get; init; } = "";
        public string RegNr { get; init; } = "";
        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public BookingStatus Status { get; init; }

        public string Label => string.IsNullOrEmpty(RegNr) ? Customer : $"{RegNr} · {Customer}";
        public string Tooltip => $"#{ShortId} {Customer}, {Vehicle} {RegNr} ({Start:d MMM} – {End:d MMM}, {Status})";
    }

    // Underlag för kalendern, används av både kalendersidan och den lilla kalendern på Overview.
    public class CalendarViewModel
    {
        public DateTime Month { get; init; }                                       // första dagen i månaden
        public int Offset { get; init; }                                           // tomma rutor före dag 1 (måndag = 0)
        public int DaysInMonth { get; init; }
        public Dictionary<int, List<CalendarEntry>> EntriesByDay { get; init; } = new();
        public List<CalendarEntry> MonthEntries { get; init; } = new();            // varje bokning en gång, sorterad på start

        public DateTime PreviousMonth => Month.AddMonths(-1);
        public DateTime NextMonth => Month.AddMonths(1);
        public string MonthName => Month.ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        public bool IsCurrentMonth => Month.Year == DateTime.Today.Year && Month.Month == DateTime.Today.Month;

        // Avbokade bokningar visas inte, de upptar ingen tid
        public static CalendarViewModel Build(IEnumerable<Booking.Booking> bookings, IUserRepository users, DateTime month)
        {
            var first = new DateTime(month.Year, month.Month, 1);
            int days = DateTime.DaysInMonth(first.Year, first.Month);
            var last = first.AddMonths(1);
            DateTime now = DateTime.Now;

            var entries = bookings
                .Where(b => b.Status != BookingStatus.Cancelled && b.StartDate < last && b.EndDate >= first)
                .OrderBy(b => b.StartDate)
                .Select(b =>
                {
                    var row = BookingRowViewModel.From(b, users, now);
                    return new CalendarEntry
                    {
                        ShortId = row.ShortId,
                        Customer = row.CustomerName,
                        Vehicle = row.Vehicle,
                        RegNr = row.RegNr,
                        Start = b.StartDate,
                        End = b.EndDate,
                        Status = b.Status
                    };
                })
                .ToList();

            var byDay = new Dictionary<int, List<CalendarEntry>>();
            for (int day = 1; day <= days; day++)
            {
                var date = new DateTime(first.Year, first.Month, day);
                var onDay = entries.Where(e => e.Start.Date <= date && e.End.Date >= date).ToList();
                if (onDay.Count > 0)
                    byDay[day] = onDay;
            }

            return new CalendarViewModel
            {
                Month = first,
                Offset = ((int)first.DayOfWeek + 6) % 7,
                DaysInMonth = days,
                EntriesByDay = byDay,
                MonthEntries = entries
            };
        }
    }
}
