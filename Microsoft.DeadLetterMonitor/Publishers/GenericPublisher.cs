using Microsoft.DeadLetterMonitor.Connectors;
using Microsoft.DeadLetterMonitor.Handlers;

namespace Microsoft.DeadLetterMonitor.Publishers {
    /// <summary>
    /// Generic publisher.
    /// </summary>
    /// <seealso cref="IGenericPublisher" />
    public class GenericPublisher : IGenericPublisher 
    {
        private readonly IConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPublisher"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public GenericPublisher(IConnection connection)
        {
            this.connection = connection;
        }

        /// <inheritdoc/>
        public void Publish(string topicName, string routingKey, IMessage message, bool createTopic = false)
        {
            var channel = connection.CreateChannel();

            if (createTopic) 
                channel.TopicDeclare(topicName);

            channel.Publish(topicName, routingKey, message);
        }
    }
}
