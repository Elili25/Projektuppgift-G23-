namespace SeventWebApplication.Models.Notifications
{
    public enum NotificationStatus
    {
        /// <summary> Waiting to be processed </summary>
        Pending,
        /// <summary> Has been sent to atleast one NotificationService for them to process </summary>
        Processed,
        /// <summary> Has been processed and sent by atleast one NotificationService </summary>
        Sent,       
        Failed,
        Cancelled
    }
}
