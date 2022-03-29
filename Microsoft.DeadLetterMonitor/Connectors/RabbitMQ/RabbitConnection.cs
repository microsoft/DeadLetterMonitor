using RabbitClient = RabbitMQ.Client;

namespace Microsoft.DeadLetterMonitor.Connectors.RabbitMQ {
    
    /// <summary>
    /// RabbitMQ implementation of IConnection
    /// </summary>
    public class RabbitConnection : IConnection {

        private readonly RabbitClient.IConnection connection;

        /// <summary>
        /// Creates a new RabbitMQ Connection
        /// </summary>
        public RabbitConnection(string hostName, string virtualHost, int port, string userName, string password) {
            
            var factory = new RabbitClient.ConnectionFactory() { 
                HostName = hostName, 
                VirtualHost = virtualHost, 
                Port = port,
                UserName = userName, 
                Password = password
            };

            connection = factory.CreateConnection();
        }

        /// <hinheritdoc/>
        public IChannel CreateChannel()
        {
            return new RabbitChannel(connection);
        }
    }
}
