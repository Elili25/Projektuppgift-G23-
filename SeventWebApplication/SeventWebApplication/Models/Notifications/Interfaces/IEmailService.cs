namespace SeventWebApplication.Models.Notifications
{
    public interface IEmailService
    {
        public void SendEmail(string email, string message);
    }
}
