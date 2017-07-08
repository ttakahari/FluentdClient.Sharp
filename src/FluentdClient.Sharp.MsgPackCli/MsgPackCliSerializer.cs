using MsgPack;
using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace FluentdClient.Sharp.MsgPackCli
{
    /// <summary>
    /// The class of MessagePack serializer using MsgPack-Cli. 
    /// </summary>
    public class MsgPackCliSerializer : IMessagePackSerializer
    {
        private static readonly DateTimeOffset _epochTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly SerializationContext _context;

        /// <summary>
        /// Create a new <see cref="MsgPackCliSerializer"/> instance.
        /// </summary>
        public MsgPackCliSerializer()
            : this(SerializationContext.Default)
        { }

        /// <summary>
        /// Create a new <see cref="MsgPackCliSerializer"/> instance.
        /// </summary>
        /// <param name="context">The serialization context information for internal serialization logic.</param>
        public MsgPackCliSerializer(SerializationContext context)
        {
            _context = context;
        }

        /// <inheritdoc cref="IMessagePackSerializer.Serialize(string, IDictionary{string, object})" />
        public byte[] Serialize(string tag, IDictionary<string, object> message)
        {
            using (var stream = new MemoryStream())
            {
                var packer = Packer.Create(stream);

                packer.PackArrayHeader(3); // [ tag, timestamp, message ]
                packer.PackString(tag);
                packer.Pack(DateTimeOffset.Now.ToUniversalTime().Subtract(_epochTime).TotalSeconds);
                packer.PackMapHeader(message);

                foreach (var item in message)
                {
                    packer.PackString(item.Key);

                    var type = item.Value?.GetType() ?? typeof(string);

                    _context.GetSerializer(type).PackTo(packer, item.Value ?? "");
                }

                return stream.ToArray();
            }
        }
    }
}
