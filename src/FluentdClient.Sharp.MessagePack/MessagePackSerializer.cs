﻿using MessagePack;
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
        private static readonly DateTimeOffset _epochTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly IFormatterResolver _resolver;

        /// <summary>
        /// Create a new <see cref="MessagePackSerializer"/> instance.
        /// </summary>
        public MessagePackSerializer()
            : this(CompositeResolver.Instance)
        { }

        /// <summary>
        /// Create a new <see cref="MessagePackSerializer"/> instance.
        /// </summary>
        /// <param name="resolver">The storage of typed serializers.</param>
        public MessagePackSerializer(IFormatterResolver resolver)
        {
            _resolver = resolver;
        }

        /// <inheritdoc cref="IMessagePackSerializer.Serialize(string, IDictionary{string, object})" />
        public byte[] Serialize(string tag, IDictionary<string, object> message)
        {
            var payload = new Payload
            {
                Tag       = tag,
                Timestamp = DateTimeOffset.Now.ToUniversalTime().Subtract(_epochTime).TotalSeconds,
                Message   = message
            };

            return global::MessagePack.MessagePackSerializer.Serialize(payload, _resolver);
        }
    }
}
