using Microsoft.DeadLetterMonitor.Handlers;
using System;
using System.Collections.Generic;

namespace Microsoft.DeadLetterMonitor.Connectors {

    /// <summary>
    /// Channel Interface
    /// </summary>
    public interface IChannel {

        /// <summary>
        /// Topic declaration
        /// </summary>
        /// <param name="name">Topic name</param>
        void TopicDeclare(string name);

        /// <summary>
        /// Queue declaration
        /// </summary>
        /// <param name="name">Queue name</param>
        /// <param name="args">Dynamic list of arguments</param>
        void QueueDeclare(string name, IDictionary<string, object>? args = null);

        /// <summary>
        /// Binding between topic and queue
        /// </summary>
        /// <param name="topicName">Topic name</param>
        /// <param name="queueName">Queue name</param>
        void QueueBind(string topicName, string queueName);

        /// <summary>
        /// Publish new message
        /// </summary>
        /// <param name="topicName">Topic where message will be published</param>
        /// <param name="routingKey">Routing Key</param>
        /// <param name="message">Message including headers and body</param>
        void Publish(string topicName, string routingKey, IMessage message);

        /// <summary>
        /// Subscribe queue for consuming messages
        /// </summary>
        /// <param name="queueName">Queue name</param>
        /// <param name="handler">Method handler to process messages</param>
        /// <param name="autoAck">Auto Ack message</param>
        IConsumer Subscribe(string queueName, Action<IMessage> handler, bool autoAck);
    }
}
