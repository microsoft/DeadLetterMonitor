using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Management.ServiceBus;

namespace Microsoft.DeadLetterMonitor.Connectors.RabbitMQ {
    
    /// <summary>
    /// ServiceBus implementation of IConnection
    /// </summary>
    public class ServiceBus : IConnection {

        // readme: https://github.com/Azure-Samples/service-bus-dotnet-management/blob/master/src/service-bus-dotnet-management/ServiceBusManagementSample.cs

        private readonly ServiceBusClient sbClient;
        private readonly ServiceBusManagementClient sbMgmtClient;

        /// <summary>
        /// Creates a new Azure ServiceBus Client and Management Client
        /// </summary>
        public ServiceBus(string connection) {
            sbClient = new ServiceBusClient(connection);
            sbMgmtClient = new ServiceBusManagementClient(new Rest.TokenCredentials("xxx"));
        }

        /// <hinheritdoc/>
        public IChannel CreateChannel()
        {
            return new ServiceBusChannel(sbClient, sbMgmtClient);
        }
    }
}
