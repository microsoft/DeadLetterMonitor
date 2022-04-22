using Azure.Messaging.ServiceBus;
using Microsoft.DeadLetterMonitor.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.DeadLetterMonitor.Connectors.RabbitMQ {
    class ServiceBusConsumer : IConsumer {

        private event EventHandler<IMessage> Received;

        /// <summary>
        /// Consumer constructor
        /// </summary>
        /// <param name="sbClient"></param>
        /// <param name="queueName">Queue to subscribe</param>
        /// <param name="handler">Handler for Message</param>
        /// <param name="autoAck">AutoAck message</param>
        public ServiceBusConsumer(ServiceBusClient sbClient, string queueName, Action<IMessage> handler, bool autoAck) {

            Received += (sender, message) => { handler(message); };

            var sbProcessor = sbClient.CreateProcessor("topicName", queueName, new ServiceBusProcessorOptions());

            // add handler to process messages
            sbProcessor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            sbProcessor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            sbProcessor.StartProcessingAsync();
        }

        // handle received messages
        private Task MessageHandler(ProcessMessageEventArgs args)
        {
            // just transform into Message and trigger event Received
            var msg = new Message(args.Message.MessageId, args.Message.EnqueuedTime.ToString(),
                                  args.Message.ContentType, "ea.Exchange", "ea.RoutingKey",
                                  args.Message.CorrelationId, new Dictionary<string, object>(), 
                                  args.Message.Body.ToArray());
            
            Received.Invoke(args, msg);

            // complete the message. messages is deleted from the subscription. 
            //await args.CompleteMessageAsync(args.Message);

            return Task.CompletedTask;
        }

        // handle any errors when receiving messages
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

    }
}
