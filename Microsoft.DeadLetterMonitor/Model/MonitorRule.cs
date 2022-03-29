namespace Microsoft.DeadLetterMonitor.Model 
{
    /// <summary>
    /// Monitor handling rule.
    /// </summary>
    public class MonitorRule 
    {
        /// <summary>
        /// Gets or sets the original exchange.
        /// </summary>
        /// <value>
        /// The original exchange.
        /// </value>
        public string OriginalExchange { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the death reason.
        /// </summary>
        /// <value>
        /// The death reason.
        /// </value>
        public string DeathReason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public string MessageType { get; set; } = string.Empty;
    }
}
