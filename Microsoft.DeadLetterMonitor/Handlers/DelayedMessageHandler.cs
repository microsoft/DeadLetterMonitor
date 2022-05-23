using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.DeadLetterMonitor.Model;
using Microsoft.DeadLetterMonitor.Publishers;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.DeadLetterMonitor.Handlers {
    /// <summary>
    /// Generic message handler.
    /// </summary>
    /// <seealso cref="IDelayedMessageHandler" />
    public class DelayedMessageHandler : IDelayedMessageHandler 
    {
        private readonly IGenericPublisher genericPublisher;
        private readonly DeadLetterMonitorOptions options;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedMessageHandler"/> class.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        /// <param name="genericPublisher">The generic bus publisher.</param>
        /// <param name="telemetryClient">Telemtry Client.</param>
        public DelayedMessageHandler(IOptions<DeadLetterMonitorOptions> options, IGenericPublisher genericPublisher, TelemetryClient telemetryClient)
        {
            this.genericPublisher = genericPublisher;
            this.options = options.Value;
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc/>
        public void HandleMessage(IMessage message)
        {
            // tracing in AppInsights in the context of the parent operation
            var telemetry = new DependencyTelemetry { Type = "Event", Name = AppDomain.CurrentDomain.FriendlyName };
            telemetry.Context.Operation.Id = message.CorrelationId;
            var operation = telemetryClient.StartOperation(telemetry);

            try
            {
                Helpers.Telemetry.Trace(telemetryClient, message.Type, "received", message.Topic, message.RoutingKey, $"Event received from {message.FirstDeathTopic} because {message.FirstDeathReason}.");

                // The original topic and is necessary to redirect the message
                if (string.IsNullOrEmpty(message.FirstDeathTopic) || string.IsNullOrEmpty(message.FirstDeathReason))
                {
                    Helpers.Telemetry.Trace(telemetryClient, message.Type, "error", 
                        message.Topic, message.RoutingKey, "Could not find original topic or reason in message.");

                    // if headers are missing send to parking lot
                    genericPublisher.Publish(options.ParkingLotTopicName, message.RoutingKey, message, true);
                    return;
                }

                // send to original queue
                genericPublisher.Publish(message.FirstDeathTopic, message.RoutingKey, message);

                Helpers.Telemetry.Trace(telemetryClient, message.Type, "resent", message.FirstDeathTopic, message.RoutingKey, "Event sent to original topic.");
            }
            finally 
            {
                telemetryClient.StopOperation(operation);
            }
        }
    }
}
