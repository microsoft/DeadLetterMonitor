using Microsoft.DeadLetterMonitor.Handlers;
using Microsoft.DeadLetterMonitor.Publishers;
using System.Collections.Generic;

namespace Microsoft.DeadLetterMonitor.Tests.Handlers {
    public class GenericPublisherMock : IGenericPublisher 
    {
        public List<string> PublishedMessages { get; set; } = new List<string>();

        public void Publish(string topicName, string routingKey, IMessage message, bool createTopic = false)
        {
            PublishedMessages.Add(topicName);
        }
    }
}
