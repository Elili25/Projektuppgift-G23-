using SeventWebApplication.Models.Booking;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Notifications
{
    public class ReminderScheduler : IReminderScheduler
    {
        private Dictionary<Guid, Reminder> _reminders = new(); // <ReminderId, Reminder>

        // How many minutes before the schedule the reminder should be sent
        public uint ScheduleBeforeMinutes;

        public ReminderScheduler( uint scheduleBeforeMinutes )
        {
            ScheduleBeforeMinutes = scheduleBeforeMinutes;
        }

        public void Schedule( BookingEntity booking )
        {
            DateTime reminderTime = booking.StartDate.Subtract( TimeSpan.FromMinutes( ScheduleBeforeMinutes ) );
            Console.WriteLine( $"Scheduled: {reminderTime}" );
            Guid bookingId = Guid.Parse( booking.BookingId );

            Reminder reminder = new( bookingId, NotificationType.PickupReminder, reminderTime );

            _reminders.Add( reminder.ReminderId, reminder );
        }

        public void Cancel( Guid bookingId )
        {
            var reminders = _reminders.Where( r => r.Value.BookingId == bookingId ).ToList();

            foreach( var reminder in reminders )
            {
                reminder.Value.Status = NotificationStatus.Cancelled;
            }
        }

        public IEnumerable<Reminder> GetPending()
        {
            return _reminders.Where( r => r.Value.Status == NotificationStatus.Pending && r.Value.ScheduleAt <= DateTime.Now  ).ToDictionary().Values;
        }

    }
}
