using Microsoft.DeadLetterMonitor.Handlers;

namespace Microsoft.DeadLetterMonitor.Publishers {
    /// <summary>
    /// Generic bus publisher.
    /// </summary>
    public interface IGenericPublisher 
    {
        /// <summary>
        /// Publishes the specified message to an exchange.
        /// </summary>
        /// <param name="topicName">The topic name.</param>
        /// <param name="routingKey">Routing Key.</param>
        /// <param name="message">Message information with headers and body.</param>
        /// <param name="createTopic">Ensure default topic creation.</param>
        void Publish(string topicName, string routingKey, IMessage message, bool createTopic = false);
    }
}
