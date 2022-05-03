using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.DeadLetterMonitor.Model;
using Microsoft.DeadLetterMonitor.Publishers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DeadLetterMonitor.Handlers {
    /// <summary>
    /// Generic message handler.
    /// </summary>
    /// <seealso cref="IGenericMessageHandler" />
    public class GenericMessageHandler : IGenericMessageHandler 
    {
        private readonly IGenericPublisher genericPublisher;
        private readonly DeadLetterMonitorOptions options;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMessageHandler"/> class.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        /// <param name="genericPublisher">The generic bus publisher.</param>
        /// <param name="telemetryClient">Telemtry Client.</param>
        public GenericMessageHandler(IOptions<DeadLetterMonitorOptions> options, IGenericPublisher genericPublisher, TelemetryClient telemetryClient)
        {
            this.genericPublisher = genericPublisher;
            this.options = options.Value;
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc/>
        public void HandleMessage(IMessage message)
        {
            // Read death information header
            var firstDeathExchange = message.GetHeaderValue("x-first-death-exchange");
            var firstDeathReason = message.GetHeaderValue("x-first-death-reason");
            var deathCount = message.GetHeaderValue("x-death-count");
            var messageType = message.Type;

            // The original exchange and is necessary to redirect the message
            if (string.IsNullOrEmpty(firstDeathExchange) || string.IsNullOrEmpty(firstDeathReason))
            {
                throw new ArgumentException("Could not find original exchange or reason in message. Possibly tried to handle a message that was not dead.");
            }

            // The death info is necessary to redirect the message
            if (string.IsNullOrEmpty(deathCount)) 
            {
                throw new ArgumentException("Could not find death header in message. Possibly tried to handle a message that was not dead.");
            }

            // tracing in AppInsights in the context of the parent operation
            var telemetry = new DependencyTelemetry { Type = "Event", Name = AppDomain.CurrentDomain.FriendlyName }; 
            telemetry.Context.Operation.Id = message.CorrelationId;
            var operation = telemetryClient.StartOperation(telemetry);

            try
            {
                int.TryParse(deathCount, out int firstDeathCount);

                Helpers.Telemetry.Trace(telemetryClient, messageType, "received", message.Topic, message.RoutingKey, $"Event received from {firstDeathExchange} because {firstDeathReason}.");

                // Discard: check if this message should be discarded
                if (RuleMatches(firstDeathExchange, messageType, firstDeathReason, options.Rules.DiscardRules))
                {
                    // Discard message - will ack the message and remove from queue
                    Helpers.Telemetry.Trace(telemetryClient, messageType, "discarded", message.Topic, message.RoutingKey, "Event discarded by rule.");
                    return;
                }

                // Park: check if max retries limit was reached or message is configured to send directly to parking lot
                // Get configuration info
                var maxRetries = options.MaxRetries;

                if (firstDeathCount >= maxRetries || RuleMatches(firstDeathExchange, messageType, firstDeathReason, options.Rules.ParkRules))
                {
                    // Send to parking lot
                    genericPublisher.Publish(options.ParkingLotExchangeName, message.RoutingKey, message, true);
                    Helpers.Telemetry.Trace(telemetryClient, messageType, "parked", message.Topic, message.RoutingKey, "Event sent to parking lot exchange.");
                    return;
                }

                // Retry: check if this message should be retried
                if (RuleMatches(firstDeathExchange, messageType, firstDeathReason, options.Rules.RetryRules))
                {
                    // Send to delayed queue
                    genericPublisher.Publish(options.DelayedExchangeName, message.RoutingKey, message);
                    Helpers.Telemetry.Trace(telemetryClient, messageType, "delayed", message.Topic, message.RoutingKey, "Event sent to delayed exchange by rule.");
                    return;
                }

                // Park message - default behaviour
                genericPublisher.Publish(options.ParkingLotExchangeName, message.RoutingKey, message, true);
                Helpers.Telemetry.Trace(telemetryClient, messageType, "parked", message.Topic, message.RoutingKey, "Event sent to parking lot by default behaviour.");
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }

        private bool RuleMatches(string? exchange, string messageType, string? reason, List<MonitorRule> rules)
        {
            // Check if rule matches current
            var exchanges = rules.Any(r => r.OriginalExchange == exchange || r.OriginalExchange == "*");
            var messageTypes = rules.Any(r => r.MessageType == messageType || r.MessageType == "*");
            var reasons = rules.Any(r => r.DeathReason == reason || r.DeathReason == "*");

            return exchanges && messageTypes && reasons;
        }
    }
}
