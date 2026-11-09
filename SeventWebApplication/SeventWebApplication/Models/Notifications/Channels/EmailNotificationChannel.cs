using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Models.Notifications
{
    public class EmailNotificationChannel : INotificationChannel
    {
        private IEmailService _emailService = new EmailService();
        private UserRepository _userRepository;

        public EmailNotificationChannel( UserRepository userRepository )
        {
            _userRepository = userRepository;
        }

        public void Send( Notification notification )
        { 
            if( notification == null )
                throw new ArgumentNullException( nameof( notification ), "Failed to send an email because notification was null" );

            User? user = _userRepository.GetById( notification.UserId );

            if( user == null )
            {
                notification.Status = NotificationStatus.Failed;
                throw new NullReferenceException( "Failed to send an email because user was null" );
            }

            if( string.IsNullOrWhiteSpace( user.Mail ) )
            {
                notification.Status = NotificationStatus.Failed;
                throw new NullReferenceException( "Failed to send an email because the email address was empty" );
            }

            _emailService.SendEmail( user.Mail, notification.Message );

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.Now;
        }

        public NotificationChannelType GetChannelType()
        {
            return NotificationChannelType.Email;
        }
    }
}
