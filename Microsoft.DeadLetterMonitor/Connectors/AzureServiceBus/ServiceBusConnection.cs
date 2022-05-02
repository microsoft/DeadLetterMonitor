using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Management.ServiceBus;

namespace Microsoft.DeadLetterMonitor.Connectors.AzureServiceBus {
    
    /// <summary>
    /// ServiceBus implementation of IConnection
    /// </summary>
    public class ServiceBusConnection : IConnection {

        private readonly ServiceBusClient sbClient;
        private readonly ServiceBusManagementClient sbMgmtClient;

        /// <summary>
        /// Creates a new Azure ServiceBus Client and Management Client
        /// </summary>
        public ServiceBusConnection(string connectionString) {
            sbClient = new ServiceBusClient(connectionString);
            sbMgmtClient = new ServiceBusManagementClient(new Rest.TokenCredentials("xxx"));
        }

        /// <hinheritdoc/>
        public IChannel CreateChannel()
        {
            return new ServiceBusChannel(sbClient, sbMgmtClient);
        }
    }
}
