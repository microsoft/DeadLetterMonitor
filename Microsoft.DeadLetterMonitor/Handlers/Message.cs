using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.DeadLetterMonitor.Handlers {

    /// <summary>
    /// Message implementation
    /// </summary>
    public class Message : IMessage {

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string Timestamp { get; set; }

        /// <inheritdoc/>
        public string Type { get; set; }

        /// <inheritdoc/>
        public string Topic { get; set; }

        /// <inheritdoc/>
        public string RoutingKey { get; set; }

        /// <inheritdoc/>
        public string CorrelationId { get; set; }

        /// <inheritdoc/>
        public int? TimeToLive { get; set; }

        /// <inheritdoc/>
        public int? Delay { get; set; }

        /// <inheritdoc/>
        public IDictionary<string, object> Headers { get; set; }

        /// <inheritdoc/>
        public byte[] Body { get; set; }

        /// <summary>
        /// Message Constructor
        /// </summary>
        public Message(string id, string timeStamp, string type, string topic, string routingKey, 
                        string correlationId, IDictionary<string, object> headers, byte[] body) {
            Id = id;
            Timestamp = timeStamp;
            Type = type;
            Topic = topic;
            RoutingKey = routingKey;
            CorrelationId = correlationId;
            Headers = headers;
            Body = body;
        }
        
        /// <inheritdoc/>
        public string? GetHeaderValue(string headerName)
        {
            if (Headers.ContainsKey(headerName) && Headers[headerName] != null)
            {
                if (Headers[headerName].GetType().Name.Equals("Byte[]"))
                    return Encoding.UTF8.GetString((byte[])Headers[headerName]);
                else 
                    return Headers[headerName].ToString();
            }
            else
            {
                return null;
            }
        }
    }
}