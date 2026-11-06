namespace SeventWebApplication.Models.Notifications
{
    public class Notification
    {
        public Notification(string message, Guid bookingId, Guid userId, NotificationType type, DateTime? scheduleAt = null)
        {
            Message = message;
            BookingId = bookingId;
            UserId = userId;
            Type = type;
            ScheduleAt = scheduleAt;
        }

        public string Message;
        public Guid NotificationId = Guid.NewGuid();
        public Guid BookingId;
        public Guid UserId;
        public NotificationType Type;
        public DateTime? ScheduleAt;
        public DateTime? SentAt = null;
        public NotificationStatus Status = NotificationStatus.Pending;
    }
}
