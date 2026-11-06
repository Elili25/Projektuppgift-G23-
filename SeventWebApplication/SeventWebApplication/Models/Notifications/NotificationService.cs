using SeventWebApplication.Models.Users;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Notifications
{
    public class NotificationService : BackgroundService, IDisposable
    {
        private INotificationRepository _notificationRepository = new NotificationRepository();
        private Dictionary<NotificationChannelType, INotificationChannel> _notificationChannels = [];

        private PeriodicTimer? _timer = null; //Timer used for calling ProcessNotifications()

        public NotificationService() 
        { 
            // Ville ha det här i Program.cs men den här är en hosted service så den verkar skapas efter app.Run()
            AddChannel<EmailNotificationChannel>();
            AddChannel<SmsNotificationChannel>();
        }

        public void AddChannel( INotificationChannel channel )
        {
            if( _notificationChannels.TryGetValue( channel.GetChannelType(), out INotificationChannel? _ ) )
                throw new InvalidOperationException( "En channel av samma typ finns redan" );

            _notificationChannels.Add( channel.GetChannelType(), channel );
        }

        // Creates a new channel and adds it to the notification channels list
        public void AddChannel<T>() where T : INotificationChannel, new()
        {
            AddChannel( new T() );
        }

        public void RemoveChannel( NotificationChannelType type )
        {
            _notificationChannels.Remove( type );
        }
        
        // Called to start processing the current messages
        public void ProcessNotifications()
        {
            IEnumerable<Notification> notifications = _notificationRepository.GetPending();

            foreach( var notification in notifications )
            {
                Send( notification );
            }
        }

        public void AddBookingConfirmedNotification( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Your booking has been confirmed", Guid.Parse( booking.BookingId ), userId, NotificationType.BookingConfirmation );
            _notificationRepository.Add( notification );
        }

        public void AddCancellationConfirmedNotification( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Your booking has successfully been cancelled", Guid.Parse( booking.BookingId ), userId, NotificationType.CancelledConfirmation );
            _notificationRepository.Add( notification );
        }

        // Den här kommer nog inte användas
        public void AddBookingChangedNotification( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Your booking has successfully been changed", Guid.Parse( booking.BookingId ), userId, NotificationType.BookingChanged );
            _notificationRepository.Add( notification );
        }

        public void AddLateReturnNotification( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Unfortunately your car was returned late", Guid.Parse( booking.BookingId ), userId, NotificationType.LateReturnInformation );
            _notificationRepository.Add( notification );
        }

        public void AddPickupReminder( BookingEntity booking )
        {
            //ToDo: Pickup Reminder
        }
        // Är det här samma? /\  \/
        public void ScheduleReminderNotification( BookingEntity booking )
        {

        }

        // Sends a message to all channels
        // It's up to every channel to check whether what they need actually exists (like phone number) 
        private void Send( Notification notification )
        {
            if( _notificationChannels.Count <= 0 )
                throw new Exception( "No notification channels found in NotificationService" );

            foreach( INotificationChannel channel in _notificationChannels.Values )
                channel.Send( notification );

            notification.Status = NotificationStatus.Processed;
        }

        // Jag har ingen aning om det här blir thread safe
        // Async funktioner och await behöver inte vara på en annan thread men att while loopen inte är blocking är lite sus
        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            if( _timer != null )
                throw new InvalidOperationException( "A Notification async task is already running" );

            Console.WriteLine( "Started NotificationService Task" );

            _timer = new( TimeSpan.FromSeconds( 1 ) );

            while( await _timer.WaitForNextTickAsync( stoppingToken ) )
            {
                ProcessNotifications();
            }
            
            _timer?.Dispose();
            _timer = null;

            Console.WriteLine( "Exited NotificationService Task" );
        }
    }
}
