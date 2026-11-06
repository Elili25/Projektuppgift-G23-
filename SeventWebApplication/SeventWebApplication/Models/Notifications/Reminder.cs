namespace SeventWebApplication.Models.Notifications
{
    public class Reminder
    {
        public Reminder( Guid bookingId, NotificationType type, DateTime scheduleAt ) 
        {
            BookingId = bookingId;
            Type = type;
            ScheduleAt = scheduleAt;
        }

        public Guid ReminderId = Guid.NewGuid();
        public Guid BookingId;
        public NotificationType Type;
        public DateTime ScheduleAt;
        public NotificationStatus Status = NotificationStatus.Pending;
    }
}
