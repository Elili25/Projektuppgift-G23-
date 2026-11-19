namespace SeventWebApplication.Models.Booking
{
    // Bakgrundsjobb som ser till att en bekräftad familjebokning utan bil får en bil senast
    // BookingManagementService.VehicleAssignDeadline (2 timmar) före start, om personalen inte hunnit välja själv.
    // Kollar en gång i minuten. BookingManagementService är scoped, så varje körning får ett eget scope.
    public class VehicleAutoAssignWorker : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<VehicleAutoAssignWorker> _logger;

        public VehicleAutoAssignWorker(IServiceScopeFactory scopeFactory, ILogger<VehicleAutoAssignWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var management = scope.ServiceProvider.GetRequiredService<BookingManagementService>();

                    int assigned = management.AutoAssignDueVehicles(DateTime.Now);
                    if (assigned > 0)
                        _logger.LogInformation("Assigned a vehicle automatically to {Count} booking(s).", assigned);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Ett fel i en körning ska inte stoppa jobbet, nästa minut provar vi igen
                    _logger.LogError(ex, "Automatic vehicle assignment failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
