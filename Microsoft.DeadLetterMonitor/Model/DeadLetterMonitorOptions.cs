namespace Microsoft.DeadLetterMonitor.Model 
{
    /// <summary>
    /// Dead Letter Monitor Configuration.
    /// </summary>
    public class DeadLetterMonitorOptions 
    {
        /// <summary>
        /// Gets or sets the maximum retries.
        /// </summary>
        /// <value>
        /// The maximum retries.
        /// </value>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the dead letter queues.
        /// </summary>
        /// <value>
        /// The dead letter queues.
        /// </value>
        public string DeadLetterQueues { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the retry delay pattern.
        /// </summary>
        /// <value>
        /// The retry delay pattern.
        /// </value>
        public int DelayValue { get; set; }

        /// <summary>
        /// Gets or sets the name of the delayed topic.
        /// </summary>
        /// <value>
        /// The name of the delayed topic.
        /// </value>
        public string DelayedTopicName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the delayed queue.
        /// </summary>
        /// <value>
        /// The name of the delayed queue.
        /// </value>
        public string DelayedQueueName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the delayed dead letter topic.
        /// </summary>
        /// <value>
        /// The name of the delayed dead letter topic.
        /// </value>
        public string DelayedDeadLetterTopicName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the delayed dead letter queue.
        /// </summary>
        /// <value>
        /// The name of the delayed dead letter queue.
        /// </value>
        public string DelayedDeadLetterQueueName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the parking lot topic.
        /// </summary>
        /// <value>
        /// The name of the parking lot topic.
        /// </value>
        public string ParkingLotTopicName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the parking lot queue.
        /// </summary>
        /// <value>
        /// The name of the parking lot queue.
        /// </value>
        public string ParkingLotQueueName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public DeadLetterMonitorRules Rules { get; set; } = null!;
    }
}
