using Microsoft.Extensions.Hosting;
using ProjectLaborBackend.Services;

public class VerificationCleanupService : BackgroundService
{
    private readonly ILogger<VerificationCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10);

    public VerificationCleanupService(ILogger<VerificationCleanupService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                VerificationStore.CleanupExpired();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while cleaning verification codes");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
