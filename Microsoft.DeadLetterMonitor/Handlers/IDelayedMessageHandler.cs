namespace Microsoft.DeadLetterMonitor.Handlers {
    /// <summary>
    /// Generic message handler.
    /// </summary>
    public interface IDelayedMessageHandler 
    {
        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="message">The message</param>
        void HandleMessage(IMessage message);
    }
}
