using System;
using System.Collections.Generic;

namespace Microsoft.DeadLetterMonitor.Handlers {

    /// <summary>
    /// Message Definition.
    /// </summary>
    public interface IMessage {

        /// <summary>
        /// Message Id
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Timestamp for message creation
        /// </summary>
        string Timestamp { get; set; }

        /// <summary>
        /// Message Type
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Message Topic
        /// </summary>
        string Topic { get; set; }

        /// <summary>
        /// Message Routing Key
        /// </summary>
        string RoutingKey { get; set; }

        /// <summary>
        /// Correlation Id to group information for telemetry
        /// </summary>
        string CorrelationId { get; set; }

        /// <summary>
        /// Time to live
        /// </summary>
        int? TimeToLive { get; set; }

        /// <summary>
        /// Delay deliver in miliseconds
        /// </summary>
        int? Delay { get; set; }

        /// <summary>
        /// First Death Topic.
        /// </summary>
        string? FirstDeathTopic { get; set; }

        /// <summary>
        /// First Death Reason.
        /// </summary>
        string? FirstDeathReason { get; set; }

        /// <summary>
        /// Death Count.
        /// </summary>
        int? DeathCount { get; set; }

        /// <summary>
        /// Message Headers
        /// </summary>
        IDictionary<string, object> Headers { get; set; }

        /// <summary>
        /// Message Body
        /// </summary>
        byte[] Body { get; set; }

        /// <summary>
        /// returns header value
        /// </summary>
        /// <param name="headerName">Header Name</param>
        /// <returns>Header Value</returns>
        string? GetHeaderValue(string headerName);
    }
}
