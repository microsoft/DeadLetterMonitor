namespace Microsoft.DeadLetterMonitor.Subscribers 
{
    /// <summary>
    /// Delayed bus subscriber.
    /// </summary>
    public interface IDelayedSubscriber 
    {
        /// <summary>
        /// Subscribes the delayed topic information.
        /// </summary>
        void Subscribe();
    }
}
