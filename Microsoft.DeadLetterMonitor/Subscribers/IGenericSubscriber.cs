namespace Microsoft.DeadLetterMonitor.Subscribers {
    /// <summary>
    /// Basic bus subscriber.
    /// </summary>
    public interface IGenericSubscriber 
    {
        /// <summary>
        /// Subscribes the specified queue.
        /// </summary>
        /// <param name="queueName">The queue information.</param>
        void Subscribe(string queueName);
    }
}
