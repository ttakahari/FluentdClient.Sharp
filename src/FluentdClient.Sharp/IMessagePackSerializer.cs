namespace FluentdClient.Sharp
{
    /// <summary>
    /// The interface that defines methods for serializing to send messages to fluentd server, with using the format of MessagePack.
    /// </summary>
    public interface IMessagePackSerializer
    {
        /// <summary>
        /// Serialize a simple message to MessagePack format to send fluentd server.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        /// <returns>The serialized message.</returns>
        byte[] Serialize(string tag, string message);

        /// <summary>
        /// Serialize a structured message to MessagePack format to send fluentd server.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The content of the message.</param>
        /// <returns>The serialized message.</returns>
        byte[] Serialize<T>(string tag, T message) where T : class;
    }
}
