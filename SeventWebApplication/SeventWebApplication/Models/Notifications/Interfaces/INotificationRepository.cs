namespace SeventWebApplication.Models.Notifications
{
    public interface INotificationRepository
    {
        public void Add(Notification notification);
        public Notification? GetById(Guid notificationId);
        public IEnumerable<Notification> GetByBookingId(Guid bookingId);
        public IEnumerable<Notification> GetPending();
        public void MarkAsSent(Notification notification);
    }
}
