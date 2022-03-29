namespace Microsoft.DeadLetterMonitor.Subscribers 
{
    /// <summary>
    /// Delayed bus subscriber.
    /// </summary>
    public interface IDelayedSubscriber 
    {
        /// <summary>
        /// Subscribes the delayed exchange information.
        /// </summary>
        void Subscribe();
    }
}
