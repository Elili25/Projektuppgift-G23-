namespace SeventWebApplication.Models.Notifications
{
    public enum NotificationStatus
    {
        Pending,    // Waiting to be processed
        Processed,  // Has been sent to atleast one NotificationService for them to process
        Sent,       // Has been processed and sent by atleast one NotificationService
        Failed,
        Cancelled
    }
}
