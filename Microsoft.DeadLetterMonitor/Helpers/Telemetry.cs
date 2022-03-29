using Microsoft.ApplicationInsights;
using System;

namespace Microsoft.DeadLetterMonitor.Helpers {
    /// <summary>
    /// Helper Methods.
    /// </summary>
    internal static class Telemetry 
    {
        /// <summary>
        /// Helper method for tracing dependency in AppInsights.
        /// </summary>
        /// <param name="telemetryClient">The telemetry client instance.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="action">Action performed.</param>
        /// <param name="exchangeName">The Exchange name.</param>
        /// <param name="routingKey">The routing key.</param>
        /// <param name="logInfo">The information to log.</param>
        internal static void Trace(TelemetryClient telemetryClient, string eventType, string action, string exchangeName, string routingKey, string logInfo)
        {
            string trace = $"Exchange: {exchangeName} | RoutingKey: {routingKey} | Info: {logInfo}";
            telemetryClient.TrackDependency("Action", $"{eventType} {action} ", trace, DateTime.UtcNow, new TimeSpan(0, 0, 0), true);
        }
    }
}
