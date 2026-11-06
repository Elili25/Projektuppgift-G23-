using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Notifications
{
    public class NotificationRepository : INotificationRepository
    {
        private Dictionary<Guid, Notification> _notifications = [];

        public void Add( Notification notification )
        {
            if( _notifications.ContainsKey( notification.NotificationId ) )
                throw new InvalidOperationException( "En Notification med samma id fanns redan i repot" );

            _notifications.Add( notification.NotificationId, notification );
        }
        public Notification? GetById( Guid notificationId )
        {
            return _notifications.GetValueOrDefault(notificationId);
        }

        public IEnumerable<Notification> GetByBookingId( Guid bookingId )
        {
            return _notifications.Values.Where( notification => notification.BookingId == bookingId );
        }

        public IEnumerable<Notification> GetPending()
        {
            return _notifications.Values.Where( notification => notification.Status == NotificationStatus.Pending );
        }

        public void CancelNotification( Notification notification )
        {
            notification.Status = NotificationStatus.Cancelled;
        }

        public void CancelNotification( Guid id )
        {
            if( _notifications.TryGetValue( id, out Notification? notification ) )
                CancelNotification( notification );
        }

        public void CancelNotification( BookingEntity booking )
        {
            Notification? notification = _notifications.FirstOrDefault( n => n.Value.BookingId == Guid.Parse( booking.BookingId ) ).Value;

            if( notification != null )
                CancelNotification( notification );
        }

        public void MarkAsSent( Notification notification )
        {
            notification.Status = NotificationStatus.Sent;
        }
    }
}
