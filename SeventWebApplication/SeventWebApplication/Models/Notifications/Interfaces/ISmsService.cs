namespace SeventWebApplication.Models.Notifications
{
    public interface ISmsService
    {
        public void SendSms(string phoneNumber, string message);
    }
}
