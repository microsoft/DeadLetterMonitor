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
            // The original exchange and is necessary to redirect the message
            if (string.IsNullOrEmpty(message.FirstDeathTopic) || string.IsNullOrEmpty(message.FirstDeathReason))
            {
                throw new ArgumentException("Could not find original exchange or reason in message. Possibly tried to handle a message that was not dead.");
            }

            // The death info is necessary to redirect the message
            if (!message.DeathCount.HasValue) 
            {
                throw new ArgumentException("Could not find death info in message. Possibly tried to handle a message that was not dead.");
            }

            // tracing in AppInsights in the context of the parent operation
            var telemetry = new DependencyTelemetry { Type = "Event", Name = AppDomain.CurrentDomain.FriendlyName }; 
            telemetry.Context.Operation.Id = message.CorrelationId;
            var operation = telemetryClient.StartOperation(telemetry);

            try
            {
                Helpers.Telemetry.Trace(telemetryClient, message.Type, "received", message.Topic, message.RoutingKey, $"Event received from {message.FirstDeathTopic} because {message.FirstDeathReason}.");

                // Discard: check if this message should be discarded
                if (RuleMatches(message.FirstDeathTopic, message.Type, message.FirstDeathReason, options.Rules.DiscardRules))
                {
                    // Discard message - will ack the message and remove from queue
                    Helpers.Telemetry.Trace(telemetryClient, message.Type, "discarded", message.Topic, message.RoutingKey, "Event discarded by rule.");
                    return;
                }

                // Park: check if max retries limit was reached or message is configured to send directly to parking lot
                // Get configuration info
                var maxRetries = options.MaxRetries;

                if (message.DeathCount >= maxRetries || RuleMatches(message.FirstDeathTopic, message.Type, message.FirstDeathReason, options.Rules.ParkRules))
                {
                    // Send to parking lot
                    genericPublisher.Publish(options.ParkingLotTopicName, message.RoutingKey, message, true);
                    Helpers.Telemetry.Trace(telemetryClient, message.Type, "parked", message.Topic, message.RoutingKey, "Event sent to parking lot exchange.");
                    return;
                }

                // Retry: check if this message should be retried
                if (RuleMatches(message.FirstDeathTopic, message.Type, message.FirstDeathReason, options.Rules.RetryRules))
                {
                    // Send to delayed queue
                    genericPublisher.Publish(options.DelayedTopicName, message.RoutingKey, message);
                    Helpers.Telemetry.Trace(telemetryClient, message.Type, "delayed", message.Topic, message.RoutingKey, "Event sent to delayed exchange by rule.");
                    return;
                }

                // Park message - default behaviour
                genericPublisher.Publish(options.ParkingLotTopicName, message.RoutingKey, message, true);
                Helpers.Telemetry.Trace(telemetryClient, message.Type, "parked", message.Topic, message.RoutingKey, "Event sent to parking lot by default behaviour.");
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
