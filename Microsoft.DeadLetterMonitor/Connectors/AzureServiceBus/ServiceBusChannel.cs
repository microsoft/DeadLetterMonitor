using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Management.ServiceBus;
using Microsoft.DeadLetterMonitor.Handlers;
using System;
using System.Collections.Generic;

namespace Microsoft.DeadLetterMonitor.Connectors.AzureServiceBus {

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
            // readme: https://github.com/Azure-Samples/service-bus-dotnet-management/blob/master/src/service-bus-dotnet-management/ServiceBusManagementSample.cs
        }

        ///<inheritdoc/>
        public void QueueDeclare(string name, IDictionary<string, object>? args = null)
        {
            // MISSING: sbMgmtClient.Queues.CreateOrUpdateAsync(...);
            // readme: https://github.com/Azure-Samples/service-bus-dotnet-management/blob/master/src/service-bus-dotnet-management/ServiceBusManagementSample.cs
        }

        ///<inheritdoc/>
        public void QueueBind(string topicName, string queueName)
        {
            // MISSING: Does this exist in ServiceBus???
            // readme: https://github.com/Azure-Samples/service-bus-dotnet-management/blob/master/src/service-bus-dotnet-management/ServiceBusManagementSample.cs
        }

        ///<inheritdoc/>
        public async void Publish(string topicName, string routingKey, IMessage message)
        {
            var sender = sbClient.CreateSender(topicName);
            var sbMessage = new ServiceBusMessage(message.Body);

            sbMessage.MessageId = message.Id;
            sbMessage.CorrelationId = message.CorrelationId;
            sbMessage.ContentType = message.Type;
            
            foreach (var header in message.Headers) {
                sbMessage.ApplicationProperties.Add(header.Key, header.Value);
            }
            
            await sender.SendMessageAsync(sbMessage);
        }

        ///<inheritdoc/>
        public ISubscriber Subscribe(string queueName, Action<IMessage> handler, bool autoAck)
        {
            return new ServiceBusSubscriber(sbClient, queueName, handler, autoAck);
        }

    }
}
