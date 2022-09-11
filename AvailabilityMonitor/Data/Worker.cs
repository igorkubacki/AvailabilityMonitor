﻿namespace AvailabilityMonitor.Services
{
    public interface IWorker
    {
        Task DoWork(CancellationToken cancellationToken);
    }
    public class Worker : IWorker
    {
        private readonly ILogger<Worker> logger;
        private int number = 0;

        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Interlocked.Increment(ref number);
                logger.LogInformation($"Worker printing number {number}");
                await Task.Delay(1000 * 5);
            }
        }
    }
}
