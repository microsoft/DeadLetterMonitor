namespace Microsoft.DeadLetterMonitor.Model 
{
    /// <summary>
    /// Monitor handling rule.
    /// </summary>
    public class MonitorRule 
    {
        /// <summary>
        /// Gets or sets the original topic.
        /// </summary>
        /// <value>
        /// The original topic.
        /// </value>
        public string OriginalTopic { get; set; } = string.Empty;

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
