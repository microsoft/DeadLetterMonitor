using Microsoft.DeadLetterMonitor.Connectors;
using Microsoft.DeadLetterMonitor.Handlers;
using Microsoft.DeadLetterMonitor.Model;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Microsoft.DeadLetterMonitor.Subscribers {
    /// <summary>
    /// Generic exchange subscriber.
    /// </summary>
    public class DelayedSubscriber : IDelayedSubscriber 
    {
        private readonly IConnection connection;
        private readonly IDelayedMessageHandler delayedMessageHandler;
        private readonly DeadLetterMonitorOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedSubscriber"/> class.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        /// <param name="connection">The persistent connection.</param>
        /// <param name="delayedMessageHandler">Generic message handler.</param>
        public DelayedSubscriber(IOptions<DeadLetterMonitorOptions> options, IConnection connection, IDelayedMessageHandler delayedMessageHandler)
        {
            this.connection = connection;
            this.delayedMessageHandler = delayedMessageHandler;
            this.options = options.Value;
        }

        /// <inheritdoc/>
        public void Subscribe()
        {
            var channel = connection.CreateChannel();

            // ensure exchanges and queues for TTL message delay
            channel.TopicDeclare(options.DelayedTopicName);
            channel.QueueDeclare(options.DelayedQueueName, 
                new Dictionary<string, object> {
                    { "x-dead-letter-exchange", options.DelayedDeadLetterTopicName },
                    { "x-message-ttl", options.DelayValue }
                });
            channel.QueueBind(options.DelayedTopicName, options.DelayedQueueName);

            channel.TopicDeclare(options.DelayedDeadLetterTopicName);
            channel.QueueDeclare(options.DelayedDeadLetterQueueName);
            channel.QueueBind(options.DelayedDeadLetterTopicName, options.DelayedDeadLetterQueueName);
            
            channel.Subscribe(options.DelayedDeadLetterQueueName, delayedMessageHandler.HandleMessage, true);
        }
    }
}
