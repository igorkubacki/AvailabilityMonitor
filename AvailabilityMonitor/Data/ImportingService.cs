using AvailabilityMonitor.Models;

namespace AvailabilityMonitor.Services
{
    public class ImportingService : IHostedService, IDisposable
    {
        private readonly ILogger<ImportingService> _logger;
        public ImportingService(ILogger<ImportingService> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {

        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
