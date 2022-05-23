using Microsoft.DeadLetterMonitor.Handlers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Microsoft.DeadLetterMonitor.Connectors.RabbitMQ {
    class RabbitSubscriber : ISubscriber {

        private readonly IModel model;

        private event EventHandler<IMessage> Received;

        /// <summary>
        /// Consumer constructor
        /// </summary>
        /// <param name="model">RabbitMQ Model</param>
        /// <param name="queueName">Queue to subscribe</param>
        /// <param name="handler">Handler for Message</param>
        /// <param name="autoAck">AutoAck message</param>
        public RabbitSubscriber(IModel model, string queueName, Action<IMessage> handler, bool autoAck) {

            this.model = model;

            Received += (sender, message) => { handler(message); };

            // now just register handler for rabbit as a wrapper
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (sender, ea) => { MessageHandler(model, ea); };

            model.BasicConsume(queueName, autoAck, consumer);
        }

        private void MessageHandler(object? sender, BasicDeliverEventArgs ea)
        {
            // just transform into Message and trigger event Received
            var msg = new Message(ea.BasicProperties.MessageId, 
                                  ea.BasicProperties.Timestamp.ToString(), 
                                  ea.BasicProperties.Type, 
                                  ea.Exchange, 
                                  ea.RoutingKey, 
                                  ea.BasicProperties.CorrelationId, 
                                  ea.BasicProperties.Headers, 
                                  ea.Body);

            // Read death information header
            msg.FirstDeathTopic = msg.GetHeaderValue("x-first-death-exchange");
            msg.FirstDeathReason = msg.GetHeaderValue("x-first-death-reason");
            msg.DeathCount = string.IsNullOrEmpty(msg.GetHeaderValue("x-death-count"))?null: (int?)int.Parse(msg.GetHeaderValue("x-death-count")!);

            try
            {
                Received.Invoke(sender, msg);
                model.BasicAck(ea.DeliveryTag, false);
            }
            catch {
                model.BasicReject(ea.DeliveryTag, true);
                throw;
            }
        }
    }
}
