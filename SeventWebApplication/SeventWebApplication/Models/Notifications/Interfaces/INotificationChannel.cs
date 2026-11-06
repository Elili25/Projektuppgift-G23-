namespace SeventWebApplication.Models.Notifications
{
    public interface INotificationChannel
    {
        public void Send( Notification notification );
        public NotificationChannelType GetChannelType();
    }
}
