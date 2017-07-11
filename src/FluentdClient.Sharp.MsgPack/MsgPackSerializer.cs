using MsgPack;
using MsgPack.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FluentdClient.Sharp.MsgPack
{
    /// <summary>
    /// The class of MessagePack serializer using MsgPack-Cli. 
    /// </summary>
    public class MsgPackSerializer : IMessagePackSerializer
    {
        private static readonly IReadOnlyDictionary<Type, Func<object, MessagePackObject>> _typedMessagePackObjectFactories = new Dictionary<Type, Func<object, MessagePackObject>>
        {
            { typeof(short)         , obj => new MessagePackObject((short)obj) },
            { typeof(int)           , obj => new MessagePackObject((int)obj) },
            { typeof(long)          , obj => new MessagePackObject((long)obj) },
            { typeof(float)         , obj => new MessagePackObject((float)obj) },
            { typeof(double)        , obj => new MessagePackObject((double)obj) },
            { typeof(ushort)        , obj => new MessagePackObject((ushort)obj) },
            { typeof(uint)          , obj => new MessagePackObject((uint)obj) },
            { typeof(ulong)         , obj => new MessagePackObject((ulong)obj) },
            { typeof(string)        , obj => new MessagePackObject((string)obj) },
            { typeof(byte)          , obj => new MessagePackObject((byte)obj) },
            { typeof(sbyte)         , obj => new MessagePackObject((sbyte)obj) },
            { typeof(byte[])        , obj => new MessagePackObject((byte[])obj) },
            { typeof(bool)          , obj => new MessagePackObject((bool)obj) },
            { typeof(DateTime)      , obj => new MessagePackObject(MessagePackConvert.FromDateTime((DateTime)obj)) },
            { typeof(DateTimeOffset), obj => new MessagePackObject(MessagePackConvert.FromDateTimeOffset((DateTimeOffset)obj)) },
        };

        private readonly SerializationContext _context;

        /// <summary>
        /// Create a new <see cref="MsgPackSerializer"/> instance.
        /// </summary>
        public MsgPackSerializer()
            : this(SerializationContext.Default)
        { }

        /// <summary>
        /// Create a new <see cref="MsgPackSerializer"/> instance.
        /// </summary>
        /// <param name="context">The serialization context information for internal serialization logic.</param>
        public MsgPackSerializer(SerializationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc cref="IMessagePackSerializer.Serialize(string, string)" />
        public byte[] Serialize(string tag, string message)
        {
            return SerializeInternal(tag, message);
        }

        /// <inheritdoc cref="IMessagePackSerializer.Serialize{T}(string, T)" />
        public byte[] Serialize<T>(string tag, T message) where T : class
        {
            return SerializeInternal(tag, message);
        }

        private byte[] SerializeInternal(string tag, object message)
        {
            var objects = new List<MessagePackObject>(3); // [ tag, timestamp, message ]

            objects.Add(tag);
            objects.Add(DateTimeOffset.Now.GetUnixTimestamp().TotalSeconds);
            objects.Add(CreateMessagePackObject(message));

            using (var stream = new MemoryStream())
            {
                var packer = Packer.Create(stream);

                packer.Pack(new MessagePackObject(objects));

                return stream.ToArray();
            }
        }

        private MessagePackObject CreateMessagePackObject(object value)
        {
            var type = (value?.GetType() ?? typeof(string)).GetTypeInfo();

            if (_typedMessagePackObjectFactories.TryGetValue(type.AsType(), out var factory))
            {
                return factory.Invoke(value);
            }

            if (type.IsEnum)
            {
                return new MessagePackObject(value.ToString());
            }

            if (value is ExpandoObject || value is IDictionary<string, object>)
            {
                var obj = (IDictionary<string, object>)value;

                var dictionary = obj.ToDictionary(x => new MessagePackObject(x.Key), x => CreateMessagePackObject(x.Value));

                return new MessagePackObject(new MessagePackObjectDictionary(dictionary));
            }

            if (type.IsArray)
            {
                var obj = (object[])value;

                var array = obj.Select(x => CreateMessagePackObject(x)).ToArray();

                return new MessagePackObject(array);
            }

            if (type.GetInterfaces().Any(x => x == typeof(IEnumerable)))
            {
                var list = new List<MessagePackObject>();

                foreach (var item in (IEnumerable)value)
                {
                    list.Add(CreateMessagePackObject(item));
                }

                return new MessagePackObject(list);
            }

            if (type.IsAnonymous())
            {
                var properties = type.GetProperties(); // heavy

                var dictionary = properties.ToDictionary(x => new MessagePackObject(x.Name), x => CreateMessagePackObject(x.GetValue(value)));

                return new MessagePackObject(new MessagePackObjectDictionary(dictionary));
            }

            var objects = TypeAccessor.GetValueGetter(type.AsType())
                .ToDictionary(x => new MessagePackObject(x.Key), x => CreateMessagePackObject(x.Value.Invoke(value)));

            return new MessagePackObject(new MessagePackObjectDictionary(objects));
        }
    }
}
