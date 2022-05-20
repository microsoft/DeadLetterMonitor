using Azure.Messaging.ServiceBus;
using Microsoft.DeadLetterMonitor.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Microsoft.DeadLetterMonitor.Connectors.AzureServiceBus {
    class ServiceBusSubscriber : ISubscriber {

        private event EventHandler<IMessage> Received;
        private readonly string routingKey;
        private readonly string queueName;

        /// <summary>
        /// Consumer constructor
        /// </summary>
        /// <param name="sbClient"></param>
        /// <param name="queueName">Queue to subscribe</param>
        /// <param name="handler">Handler for Message</param>
        /// <param name="autoAck">AutoAck message</param>
        public ServiceBusSubscriber(ServiceBusClient sbClient, string queueName, Action<IMessage> handler, bool autoAck) {

            Received += (sender, message) => { handler(message); };
            this.routingKey = queueName;
            this.queueName = queueName;

            var sbProcessor = sbClient.CreateProcessor(queueName, new ServiceBusProcessorOptions { AutoCompleteMessages = autoAck, SubQueue = SubQueue.DeadLetter });

            // add handler to process messages
            sbProcessor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            sbProcessor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            sbProcessor.StartProcessingAsync();
        }

        // handle received messages
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            // just transform into Message and trigger event Received
            var msg = new Message(args.Message.MessageId, 
                                  args.Message.EnqueuedTime.ToString(),
                                  args.Message.ContentType, 
                                  string.Empty,
                                  routingKey,
                                  args.Message.CorrelationId,
                                  (IDictionary<string, object>)args.Message.ApplicationProperties, 
                                  args.Message.Body.ToArray());

            // Read death information header
            msg.FirstDeathTopic = this.queueName;
            msg.FirstDeathReason = args.Message.DeadLetterReason;
            msg.DeathCount = args.Message.DeliveryCount;

            Received.Invoke(args, msg);

            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

    }
}
