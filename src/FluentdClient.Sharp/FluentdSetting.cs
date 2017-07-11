using System;

namespace FluentdClient.Sharp
{
    /// <summary>
    /// The class that defines settings for connecting to fluentd server.
    /// </summary>
    public class FluentdSetting
    {
        /// <summary>
        /// The host of fluentd server.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The port of fluentd server.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// The MassagePack serializer.
        /// </summary>
        public IMessagePackSerializer Serializer { get; }

        /// <summary>
        /// The timeout value for connecting to fluentd server. (Milliseconds)
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// How to handle exceptions when connecting to fluentd server.
        /// </summary>
        public Action<Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// Create a new <see cref="FluentdSetting"/> instance.
        /// </summary>
        /// <param name="host">The host of fluentd server.</param>
        /// <param name="port">The port of fluentd server.</param>
        /// <param name="serializer">The MessagePack serializer.</param>
        public FluentdSetting(string host, int port, IMessagePackSerializer serializer)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            Host       = host;
            Port       = port;
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }
    }
}
