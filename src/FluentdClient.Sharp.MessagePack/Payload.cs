/*
 * Reference to https://gist.github.com/neuecc/1f9eff356cc2b9ca709c7dc5bab7b2a1
 * Thanks for @neuecc.
 */
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentdClient.Sharp.MessagePack
{
    /// <summary>
    /// The class that defines a payload for sending to fluentd.
    /// </summary>
    [MessagePackObject]
    public class Payload
    {
        /// <summary>
        /// Tag.
        /// </summary>
        [Key(0)]
        public string Tag { get; set; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        [Key(1)]
        public double Timestamp { get; set; }

        /// <summary>
        /// Message.
        /// </summary>
        [Key(2)]
        public IDictionary<string, object> Message { get; set; }
    }

    /// <summary>
    /// The class that resolves the MessagePack format of <see cref="Payload"/>.
    /// </summary>
    public sealed class PayloadFormtterResolver : IFormatterResolver
    {
        /// <summary>
        /// Get the current instance of <see cref="PayloadFormtterResolver"/>.
        /// </summary>
        public static IFormatterResolver Instance { get; }

        static PayloadFormtterResolver()
        {
            Instance = new PayloadFormtterResolver();
        }

        private PayloadFormtterResolver() { }

        /// <inheritdoc cref="IFormatterResolver.GetFormatter{T}" />
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        static class FormatterCache<T>
        {
            public static IMessagePackFormatter<T> Formatter { get; }

            static FormatterCache()
            {
                Formatter = (IMessagePackFormatter<T>)PayloadFormatter.Instance;
            }
        }
    }
    
    /// <summary>
    /// The class that defines the MessagePack format of <see cref="Payload"/>.
    /// </summary>
    public class PayloadFormatter : IMessagePackFormatter<Payload>
    {
        private static readonly StaticNullableFormatter<DateTime> _nullableDateTimeFormatter;
        private static readonly ArrayFormatter<DateTime> _dateTimeArrayFormatter;
        private static readonly StaticNullableFormatter<DateTimeOffset> _nullableDateTimeOffsetFormatter;
        private static readonly ArrayFormatter<DateTimeOffset> _dateTimeOffsetArrayFormatter;

        /// <summary>
        /// Get the current instance of <see cref="PayloadFormatter"/>.
        /// </summary>
        public static PayloadFormatter Instance { get; }

        static PayloadFormatter()
        {
            _nullableDateTimeFormatter       = new StaticNullableFormatter<DateTime>(UnixTimestampDateTimeFormatter.Instance);
            _dateTimeArrayFormatter          = new ArrayFormatter<DateTime>();
            _nullableDateTimeOffsetFormatter = new StaticNullableFormatter<DateTimeOffset>(UnixTimestampDateTimeOffsetFormatter.Instance);
            _dateTimeOffsetArrayFormatter    = new ArrayFormatter<DateTimeOffset>();

            Instance = new PayloadFormatter();
        }

        /// <inheritdoc cref="IMessagePackFormatter{T}.Serialize(ref byte[], int, T, IFormatterResolver)" />
        public int Serialize(ref byte[] bytes, int offset, Payload value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;

            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 3);
            offset += MessagePackBinary.WriteString(ref bytes, offset, value.Tag);
            offset += MessagePackBinary.WriteDouble(ref bytes, offset, value.Timestamp);
            offset += MessagePackBinary.WriteMapHeader(ref bytes, offset, value.Message.Count);

            foreach (var item in value.Message)
            {
                offset += MessagePackBinary.WriteString(ref bytes, offset, item.Key);

                if (item.Value == null)
                {
                    offset += NilFormatter.Instance.Serialize(ref bytes, offset, Nil.Default, formatterResolver);
                    continue;
                }

                var type     = item.Value.GetType();
                var typeInfo = type.GetTypeInfo();
                
                if (item.Value is DateTime dateTime)
                {
                    offset += UnixTimestampDateTimeFormatter.Instance.Serialize(ref bytes, offset, dateTime, formatterResolver);
                }
                else if (item.Value is DateTime?) // Nullable-type can't use pattern-matching.
                {
                    offset += _nullableDateTimeFormatter.Serialize(ref bytes, offset, (DateTime?)item.Value, formatterResolver);
                }
                else if (item.Value is DateTime[] dateTimes)
                {
                    offset += _dateTimeArrayFormatter.Serialize(ref bytes, offset, dateTimes, formatterResolver);
                }
                else if (item.Value is DateTimeOffset dateTimeOffset)
                {
                    offset += UnixTimestampDateTimeOffsetFormatter.Instance.Serialize(ref bytes, offset, dateTimeOffset, formatterResolver);
                }
                else if (item.Value is DateTimeOffset?) // Nullable-type can't use pattern-matching.
                {
                    offset += _nullableDateTimeOffsetFormatter.Serialize(ref bytes, offset, (DateTimeOffset?)item.Value, formatterResolver);
                }
                else if (item.Value is DateTimeOffset[] dateTimeOffsets)
                {
                    offset += _dateTimeOffsetArrayFormatter.Serialize(ref bytes, offset, dateTimeOffsets, formatterResolver);
                }
                else
                {
                    // can't setrialize object types.

                    offset += PrimitiveObjectFormatter.Instance.Serialize(ref bytes, offset, item.Value, formatterResolver);
                }
            }

            return offset - startOffset;
        }

        /// <inheritdoc cref="IMessagePackFormatter{T}.Deserialize(byte[], int, IFormatterResolver, out int)" />
        public Payload Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            throw new NotSupportedException();
        }
    }
}
