using Microsoft.DeadLetterMonitor.Connectors;
using Microsoft.DeadLetterMonitor.Handlers;

namespace Microsoft.DeadLetterMonitor.Subscribers {

    /// <summary>
    /// Generic subscriber.
    /// </summary>
    public class GenericSubscriber : IGenericSubscriber 
    {
        private readonly IConnection connection;
        private readonly IGenericMessageHandler genericMessageHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSubscriber"/> class.
        /// </summary>
        /// <param name="connection">The persistent connection.</param>
        /// <param name="genericMessageHandler">Generic message handler.</param>
        public GenericSubscriber(IConnection connection, IGenericMessageHandler genericMessageHandler)
        {
            this.connection = connection;
            this.genericMessageHandler = genericMessageHandler;
        }

        /// <inheritdoc/>
        public void Subscribe(string queueName)
        {
            var channel = connection.CreateChannel();

            // ensure queues exists
            channel.QueueDeclare(queueName);

            channel.Subscribe(queueName, genericMessageHandler.HandleMessage, true);
        }
    }
}
