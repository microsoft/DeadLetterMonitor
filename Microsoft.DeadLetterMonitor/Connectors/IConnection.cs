namespace Microsoft.DeadLetterMonitor.Connectors {

    /// <summary>
    /// Interface for Connection
    /// </summary>
    public interface IConnection {

        /// <summary>
        /// Create Channel using connection.
        /// </summary>
        /// <returns></returns>
        IChannel CreateChannel();
    }
}
