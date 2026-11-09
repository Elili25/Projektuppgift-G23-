using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Models.Notifications
{
    public class SmsNotificationChannel : INotificationChannel
    {
        private ISmsService _smsService = new SmsService();
        UserRepository _userRepository;

        public SmsNotificationChannel( UserRepository userRepository )
        {
            _userRepository = userRepository;
        }

        public void Send( Notification notification )
        {
            if( notification == null )
                throw new ArgumentNullException( nameof( notification ), "Failed to send a sms because notification was null" );

            User? user = _userRepository.GetById( notification.UserId );

            if( user == null )
            {
                notification.Status = NotificationStatus.Failed;
                throw new NullReferenceException( "Failed to send a sms because user was null" );
            }

            if( string.IsNullOrWhiteSpace( user.Phone ) )
                throw new ArgumentNullException( "Failed to send a sms because the phone number was empty" );

            _smsService.SendSms( user.Phone, notification.Message );
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.Now;
        }

        public NotificationChannelType GetChannelType()
        {
            return NotificationChannelType.Sms;
        }

    }
}
