namespace SeventWebApplication.Models.Notifications
{
    public class SmsService : ISmsService
    {
        public void SendSms( string phoneNumber, string message )
        {
            Console.WriteLine( "--------------------------" );
            Console.WriteLine( "Notification sent via: Sms" );
            Console.WriteLine( $"To: {phoneNumber}" );
            Console.WriteLine( $"Message: {message}" );
            Console.WriteLine( "--------------------------" );
        }
    }
}
