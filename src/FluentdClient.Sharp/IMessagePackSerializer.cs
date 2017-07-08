using System.Collections.Generic;

namespace FluentdClient.Sharp
{
    /// <summary>
    /// The interface that defines methods for serializing to send messages to fluentd server, with using the format of MessagePack.
    /// </summary>
    public interface IMessagePackSerializer
    {
        /// <summary>
        /// Serialize a message to the format MessagePack to send fluentd server.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The content of the message.</param>
        /// <returns>The serialized message.</returns>
        byte[] Serialize(string tag, IDictionary<string, object> message);
    }
}
