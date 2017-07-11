using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;

namespace FluentdClient.Sharp.MessagePack
{
    /// <summary>
    /// The class of MessagePack serializer using MessagePack. 
    /// </summary>
    public class MessagePackSerializer : IMessagePackSerializer
    {
        /// <summary>
        /// Create a new <see cref="MessagePackSerializer"/> instance.
        /// </summary>
        public MessagePackSerializer()
            : this(TypelessContractlessStandardResolver.Instance)
        { }

        /// <summary>
        /// Create a new <see cref="MessagePackSerializer"/> instance.
        /// </summary>
        /// <param name="resolver">The storage of typed serializers.</param>
        public MessagePackSerializer(IFormatterResolver resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            MultipleFormatterResolver.AddFormatterResolver(resolver);
        }

        public byte[] Serialize(string tag, string message)
        {
            var payload = new Payload
            {
                Tag       = tag,
                Timestamp = DateTimeOffset.Now.GetUnixTimestamp().TotalSeconds,
                Message   = message
            };

            return global::MessagePack.MessagePackSerializer.Serialize(payload, MultipleFormatterResolver.Instance);
        }

        public byte[] Serialize<T>(string tag, T message) where T : class
        {
            var payload = new Payload
            {
                Tag       = tag,
                Timestamp = DateTimeOffset.Now.GetUnixTimestamp().TotalSeconds,
                Message   = message
            };

            var bytes = global::MessagePack.MessagePackSerializer.Serialize(payload, MultipleFormatterResolver.Instance);

            var json = global::MessagePack.MessagePackSerializer.ToJson(bytes);

            return bytes;
        }
    }

    /// <summary>
    /// The class that resolves the multiple MessagePack format including <see cref="PayloadFormtterResolver"/>.s
    /// </summary>
    public sealed class MultipleFormatterResolver : IFormatterResolver
    {
        private static readonly IList<IFormatterResolver> _resolvers;

        /// <summary>
        /// Get the current instance of <see cref="MultipleFormatterResolver"/>.
        /// </summary>
        public static MultipleFormatterResolver Instance { get; }

        static MultipleFormatterResolver()
        {
            _resolvers = new List<IFormatterResolver> { UnixTimestampFormetterResolver.Instance, PayloadFormtterResolver.Instance };

            Instance = new MultipleFormatterResolver();
        }

        private MultipleFormatterResolver() { }

        /// <summary>
        /// Add a instance of <see cref="IFormatterResolver"/>.
        /// </summary>
        /// <param name="resolver">The additional formatter resolver.</param>
        public static void AddFormatterResolver(IFormatterResolver resolver)
        {
            if (!_resolvers.Contains(resolver))
            {
                _resolvers.Add(resolver);
            }
        }
        
        /// <inheritdoc cref="IFormatterResolver.GetFormatter{T}" />
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static IMessagePackFormatter<T> Formatter { get; }

            static FormatterCache()
            {
                foreach (var item in _resolvers)
                {
                    var f = item.GetFormatter<T>();

                    if (f != null)
                    {
                        Formatter = f;
                        return;
                    }
                }
            }
        }
    }
}
