using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Models.Notifications
{
    public class EmailNotificationChannel : INotificationChannel
    {
        private IEmailService _emailService = new EmailService();
        private UserRepository _userRepository = new();

        public void Send( Notification notification )
        {
            User? user = _userRepository.GetById( notification.UserId );
            string email = user != null ? user.Mail : ""; // bara för den klagade på nullreference
            _emailService.SendEmail( email, notification.Message );
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.Now;
        }

        public NotificationChannelType GetChannelType()
        {
            return NotificationChannelType.Email;
        }
    }
}
