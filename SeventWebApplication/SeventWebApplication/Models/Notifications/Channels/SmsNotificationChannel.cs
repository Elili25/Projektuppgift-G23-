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
            User? user = _userRepository.GetById( notification.UserId );
            string phone = user != null ? user.Phone : ""; // bara för den klagade på nullreference
            _smsService.SendSms( phone, notification.Message );
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.Now;
        }

        public NotificationChannelType GetChannelType()
        {
            return NotificationChannelType.Sms;
        }

    }
}
