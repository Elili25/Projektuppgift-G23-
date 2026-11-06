using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Notifications
{
    public interface IReminderScheduler
    {
        public void Schedule( BookingEntity booking );
        public void Cancel( Guid bookingId );
  
        /// <returns> All <see cref="Reminder"/>'s that are <see cref="NotificationStatus.Pending"/> and whos <see cref="Reminder.ScheduleAt"/> time has passed </returns>
        public IEnumerable<Reminder> GetPending();
    }
}
