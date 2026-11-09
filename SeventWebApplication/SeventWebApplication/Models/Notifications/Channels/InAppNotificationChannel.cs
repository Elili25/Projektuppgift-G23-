using SeventWebApplication.Models.Users;
using static System.Net.WebRequestMethods;

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
            if( notification == null )
                throw new ArgumentNullException( nameof( notification ), "Failed to send an in app notification because notification was null" );

            User? user = _userRepository.GetById( notification.UserId );

            if( user == null )
            { 
                notification.Status = NotificationStatus.Failed;
                throw new NullReferenceException( "Failed to send an in app notification because user was null");
            }

            Console.WriteLine( "--------------------------" );
            Console.WriteLine( "Notification sent via: InApp" );
            Console.WriteLine( $"To: {user.Name}" );
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
