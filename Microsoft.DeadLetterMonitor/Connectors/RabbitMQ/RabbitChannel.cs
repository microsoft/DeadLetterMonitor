using Microsoft.DeadLetterMonitor.Handlers;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using RabbitClient = RabbitMQ.Client;

namespace Microsoft.DeadLetterMonitor.Connectors.RabbitMQ {

    /// <summary>
    /// RabbitMQ implementation for IChannel
    /// </summary>
    public class RabbitChannel : IChannel {

        private readonly IModel model;

        /// <summary>
        /// Channel constructor
        /// </summary>
        /// <param name="connection"></param>
        public RabbitChannel(RabbitClient.IConnection connection) {
            model = connection.CreateModel();
        }

        ///<inheritdoc/>
        public void TopicDeclare(string name)
        {
            model.ExchangeDeclare(name, ExchangeType.Fanout, true);
        }

        ///<inheritdoc/>
        public void QueueDeclare(string name, IDictionary<string, object>? args = null)
        {
            model.QueueDeclare(name, true, false, false, args);
        }

        ///<inheritdoc/>
        public void QueueBind(string topicName, string queueName)
        {
            model.QueueBind(queueName, topicName, string.Empty, null);
        }

        ///<inheritdoc/>
        public void Publish(string topicName, string routingKey, IMessage message)
        {
            var props = model.CreateBasicProperties();
            props.Headers = message.Headers;
            if (message.Id != null) props.MessageId = message.Id;
            if (message.Type != null) props.Type = message.Type;
            if (message.CorrelationId != null) props.CorrelationId = message.CorrelationId;
            props.DeliveryMode = 2;

            model.BasicPublish(topicName, routingKey, true, props, message.Body);
        }

        ///<inheritdoc/>
        public IConsumer Subscribe(string queueName, Action<IMessage> handler, bool autoAck)
        {
            return new RabbitConsumer(model, queueName, handler, autoAck);
        }

    }
}
