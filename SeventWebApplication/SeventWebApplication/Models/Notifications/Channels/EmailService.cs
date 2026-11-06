namespace SeventWebApplication.Models.Notifications
{
    public class EmailService : IEmailService
    {
        public void SendEmail( string email, string message )
        {
            Console.WriteLine( "--------------------------" );
            Console.WriteLine( "Notification sent via: Email" );
            Console.WriteLine( $"To: {email}" );
            Console.WriteLine( $"Message: {message}" );
            Console.WriteLine( "--------------------------" );
        }
    }
}
