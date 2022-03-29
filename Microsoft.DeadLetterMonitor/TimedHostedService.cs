using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DeadLetterMonitor.Subscribers;
using Microsoft.DeadLetterMonitor.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.DeadLetterMonitor.EventProcessor 
{
    /// <summary>
    /// Time hosted service.
    /// </summary>
    /// <seealso cref="IHostedService" />
    /// <seealso cref="IDisposable" />
    public class TimedHostedService : IHostedService, IDisposable 
    {
        private readonly ILogger logger;
        private readonly IGenericSubscriber genericSubscriber;
        private readonly IDelayedSubscriber delayedSubscriber;
        private readonly DeadLetterMonitorOptions options;

        private Timer? timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedHostedService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">Configuration options.</param>
        /// <param name="genericSubscriber">Generic subscriber.</param>
        /// <param name="delayedSubscriber">Delayed subscriber.</param>
        public TimedHostedService(ILogger<TimedHostedService> logger, IOptions<DeadLetterMonitorOptions> options, 
                                IGenericSubscriber genericSubscriber, IDelayedSubscriber delayedSubscriber)
        {
            this.logger = logger;
            this.options = options.Value;
            this.genericSubscriber = genericSubscriber;
            this.delayedSubscriber = delayedSubscriber;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Dead Letter Service is starting.");
            System.Diagnostics.Process.Start("ipconfig.exe", "/all");

            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

            try
            {
                var queues = options.DeadLetterQueues.Split(",");

                foreach (var queue in queues)
                {
                    genericSubscriber.Subscribe(queue);
                }

                delayedSubscriber.Subscribe();

                // heartbeat each 60 seconds - can be pushed for infinite with "Thread.Sleep(Timeout.Infinite)"
                timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred initializing Dead Letter subscribers.");
            }

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            logger.LogInformation("Dead Letter Service is running.");
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Dead Letter Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Custom dispose implementation.
        /// </summary>
        /// <param name="disposing">disposing in progress.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Dispose();
            }
        }
    }
}
