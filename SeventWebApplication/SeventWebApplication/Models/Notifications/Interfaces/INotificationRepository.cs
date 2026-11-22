using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Notifications
{
    public interface INotificationRepository
    {
        void Add(Notification notification);
        Notification? GetById(Guid notificationId);
        IEnumerable<Notification> GetByBookingId(Guid bookingId);
        IEnumerable<Notification> GetPending();
        void MarkAsSent(Notification notification);

        // Avbryter notifikationer som inte har skickats än
        void CancelNotification(Notification notification);
        void CancelNotification(Guid notificationId);
        void CancelNotification(BookingEntity booking);
    }
}


