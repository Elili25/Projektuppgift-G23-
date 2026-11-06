using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Models.Notifications
{
    public class InAppNotificationChannel : INotificationChannel
    {
        private UserRepository _userRepository;

        public InAppNotificationChannel( UserRepository userRepository )
        {
            _userRepository = userRepository;
        }

        public void Send( Notification notification )
        {
            User? user = _userRepository.GetById( notification.UserId );

            Console.WriteLine( "--------------------------" );
            Console.WriteLine( "Notification sent via: InApp" );
            Console.WriteLine( $"To: {user?.Name}" );
            Console.WriteLine( $"Message: {notification.Message}" );
            Console.WriteLine( "--------------------------" );

            notification.Status = NotificationStatus.Sent;
            if( notification.SentAt == null)
                notification.SentAt = DateTime.Now;
        }

        public NotificationChannelType GetChannelType()
        {
            return NotificationChannelType.InApp;
        }
    }
}
