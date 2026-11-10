using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Users;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Notifications
{
    public class NotificationService : BackgroundService
    {
        private readonly INotificationRepository _notificationRepository = new NotificationRepository();
        private readonly Dictionary<NotificationChannelType, INotificationChannel> _notificationChannels = [];
        private readonly IReminderScheduler _scheduler;

        private PeriodicTimer? _timer = null; //Timer used for calling ProcessNotifications()

        private BookingRepository _bookingRepository;

        public NotificationService( BookingRepository bookingRepository ) 
        {
            _scheduler = new ReminderScheduler( 30 /*minutes*/);
            _bookingRepository = bookingRepository;
        }

        public void AddChannel( INotificationChannel channel )
        {
            if( _notificationChannels.TryGetValue( channel.GetChannelType(), out INotificationChannel? _ ) )
                throw new InvalidOperationException( "En channel av samma typ finns redan" );

            _notificationChannels.Add( channel.GetChannelType(), channel );
        }

        public void RemoveChannel( NotificationChannelType type )
        {
            _notificationChannels.Remove( type );
        }
        
        // Called to start processing the current messages
        private void ProcessNotifications()
        {
            var notifications = _notificationRepository.GetPending();

            foreach( var notification in notifications )
            {
                Send( notification );
            }

            var scheduledReminder = _scheduler.GetPending();

            foreach( var reminder in scheduledReminder )
            {
                SendReminder( reminder );
            }
        }

        public void AddBookingConfirmed( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Your booking has been confirmed", Guid.Parse( booking.BookingId ), userId, NotificationType.BookingConfirmation );
            _notificationRepository.Add( notification );
        }

        public void AddCancellationConfirmed( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Your booking has successfully been cancelled", Guid.Parse( booking.BookingId ), userId, NotificationType.CancelledConfirmation );
            _notificationRepository.Add( notification );
        }

        // Den här kommer nog inte användas
        public void AddBookingChanged( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Your booking has successfully been changed", Guid.Parse( booking.BookingId ), userId, NotificationType.BookingChanged );
            _notificationRepository.Add( notification );
        }

        public void AddRentalReturned( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Thank you for choosing Sevent, would you like to give us 5 start on welp.com", Guid.Parse( booking.BookingId ), userId, NotificationType.RentalReturn );
            _notificationRepository.Add( notification );
        }

        public void AddLateReturn( BookingEntity booking )
        {
            Guid userId = Guid.Parse( booking.UserId );

            Notification notification = new( "Unfortunately your car was returned late", Guid.Parse( booking.BookingId ), userId, NotificationType.LateReturnInformation );
            _notificationRepository.Add( notification );
        }

        public void AddPickupReminder( BookingEntity booking )
        {
            _scheduler.Schedule( booking );
        }

        // Sends a message to all channels
        // It's up to every channel to check whether what they need actually exists (like phone number) 
        private void Send( Notification notification )
        {
            if( _notificationChannels.Count <= 0 )
                throw new Exception( "No notification channels found in NotificationService" );

            notification.Status = NotificationStatus.Processed;

            foreach( INotificationChannel channel in _notificationChannels.Values )
                channel.Send( notification );

        }

        private void SendReminder( Reminder reminder )
        {
            if( !_notificationChannels.TryGetValue( NotificationChannelType.InApp, out var inApp ) )
                throw new Exception( "No InApp notification channel was found" );

            BookingEntity? booking = _bookingRepository.GetById( reminder.BookingId.ToString() );

            if( booking == null )
            {
                reminder.Status = NotificationStatus.Cancelled;
                return;
            }

            // ToDo: Lite skumt att göra såhär när vi har en scheduleAt i Notification, kanske får fixa sen
            Notification reminderNotification = new(
                $"Reminder: your booking for today is at {reminder.ScheduleAt.Hour}:{reminder.ScheduleAt.Minute}",
                reminder.BookingId,
                Guid.Parse( booking.UserId ),
                NotificationType.PickupReminder );
                

            reminder.Status = NotificationStatus.Processed;
            inApp.Send( reminderNotification );
        }

        // Jag har ingen aning om det här blir thread safe
        // Async funktioner och await behöver inte vara på en annan thread men att while loopen inte är blocking är lite sus
        // Vad jag kan se i output så skapas ingen ny thread
        // Called after app.Run() in main
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
