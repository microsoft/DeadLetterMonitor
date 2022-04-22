﻿using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Management.ServiceBus;
using Microsoft.DeadLetterMonitor.Handlers;
using System;
using System.Collections.Generic;

namespace Microsoft.DeadLetterMonitor.Connectors.RabbitMQ {

    /// <summary>
    /// RabbitMQ implementation for IChannel
    /// </summary>
    public class ServiceBusChannel : IChannel {

        private readonly ServiceBusClient sbClient;
        private readonly ServiceBusManagementClient sbMgmtClient;

        /// <summary>
        /// Channel constructor
        /// </summary>
        /// <param name="sbClient"></param>
        /// <param name="sbMgmtClient"></param>
        public ServiceBusChannel(ServiceBusClient sbClient, ServiceBusManagementClient sbMgmtClient) {
            this.sbClient = sbClient;
            this.sbMgmtClient = sbMgmtClient;
        }

        ///<inheritdoc/>
        public void TopicDeclare(string name)
        {
            // MISSING: sbMgmtClient.Topics.CreateOrUpdateAsync(...);
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public void QueueDeclare(string name, IDictionary<string, object>? args = null)
        {
            // MISSING: sbMgmtClient.Queues.CreateOrUpdateAsync(...);
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public void QueueBind(string topicName, string queueName)
        {
            // MISSING: Does this exist in ServiceBus???
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public void Publish(string topicName, string routingKey, IMessage message)
        {
            var sender = sbClient.CreateSender(topicName);
            var sbMessage = new ServiceBusMessage(message.Body);

            // ATT: missing routing key
            // model.BasicPublish(topicName, routingKey, true, props, message.Body);
            sender.SendMessageAsync(sbMessage);
        }

        ///<inheritdoc/>
        public IConsumer Subscribe(string queueName, Action<IMessage> handler, bool autoAck)
        {
            return new ServiceBusConsumer(sbClient, queueName, handler, autoAck);
        }

    }
}