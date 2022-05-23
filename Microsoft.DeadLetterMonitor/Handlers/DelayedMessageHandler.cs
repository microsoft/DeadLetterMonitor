using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.DeadLetterMonitor.Publishers;
using System;

namespace Microsoft.DeadLetterMonitor.Handlers {
    /// <summary>
    /// Generic message handler.
    /// </summary>
    /// <seealso cref="IDelayedMessageHandler" />
    public class DelayedMessageHandler : IDelayedMessageHandler 
    {
        private readonly IGenericPublisher genericPublisher;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedMessageHandler"/> class.
        /// </summary>
        /// <param name="genericPublisher">The generic bus publisher.</param>
        /// <param name="telemetryClient">Telemtry Client.</param>
        public DelayedMessageHandler(IGenericPublisher genericPublisher, TelemetryClient telemetryClient)
        {
            this.genericPublisher = genericPublisher;
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc/>
        public void HandleMessage(IMessage message)
        {
            // The original topic and is necessary to redirect the message
            if (string.IsNullOrEmpty(message.FirstDeathTopic) || string.IsNullOrEmpty(message.FirstDeathReason))
            {
                throw new ArgumentException("Could not find original topic or reason in message. Possibly tryied to handle a message that was not dead.");
            }

            // tracing in AppInsights in the context of the parent operation
            var telemetry = new DependencyTelemetry { Type = "Event", Name = AppDomain.CurrentDomain.FriendlyName };
            telemetry.Context.Operation.Id = message.CorrelationId;
            var operation = telemetryClient.StartOperation(telemetry);

            try
            {
                Helpers.Telemetry.Trace(telemetryClient, message.Type, "received", message.Topic, message.RoutingKey, $"Event received from {message.FirstDeathTopic} because {message.FirstDeathReason}.");

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
