/*
 * Reference to https://gist.github.com/neuecc/b03ad445bb39572283fe9dd616cc3d69
 * Thanks for @neuecc.
 */
using MessagePack;
using MessagePack.Formatters;
using System;

namespace FluentdClient.Sharp.MessagePack
{
    /// <summary>
    /// The class that defines the MessagePack format of <see cref="DateTime"/> as UnixTimestamp.
    /// </summary>
    public sealed class UnixTimestampDateTimeFormatter : IMessagePackFormatter<DateTime>
    {
        /// <summary>
        /// Get the current instance of <see cref="UnixTimestampDateTimeFormatter"/>.
        /// </summary>
        public static UnixTimestampDateTimeFormatter Instance { get; }

        static UnixTimestampDateTimeFormatter()
        {
            Instance = new UnixTimestampDateTimeFormatter();
        }

        private UnixTimestampDateTimeFormatter() { }

        /// <inheritdoc cref="IMessagePackFormatter.Serialize(ref byte[], int, T, IFormatterResolver)" />
        public int Serialize(ref byte[] bytes, int offset, DateTime value, IFormatterResolver formatterResolver)
        {
            var unixTimestamp = value.GetUnixTimestamp().TotalSeconds;

            return MessagePackBinary.WriteDouble(ref bytes, offset, unixTimestamp);
        }

        /// <inheritdoc cref="IMessagePackFormatter.Deserialize(byte[], int, IFormatterResolver, out int)" />
        public DateTime Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.GetMessagePackType(bytes, offset) == MessagePackType.Extension)
            {
                return DateTimeFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out readSize);
            }

            var unixTimestamp = MessagePackBinary.ReadInt64(bytes, offset, out readSize);

            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
        }
    }

    /// <summary>
    /// The class that defines the MessagePack format of <see cref="DateTimeOffset"/> as UnixTimestamp.
    /// </summary>
    public sealed class UnixTimestampDateTimeOffsetFormatter : IMessagePackFormatter<DateTimeOffset>
    {
        /// <summary>
        /// Get the current instance of <see cref="UnixTimestampDateTimeOffsetFormatter"/>.
        /// </summary>
        public static UnixTimestampDateTimeOffsetFormatter Instance { get; }

        static UnixTimestampDateTimeOffsetFormatter()
        {
            Instance = new UnixTimestampDateTimeOffsetFormatter();
        }

        private UnixTimestampDateTimeOffsetFormatter() { }

        /// <inheritdoc cref="IMessagePackFormatter.Serialize(ref byte[], int, T, IFormatterResolver)" />
        public int Serialize(ref byte[] bytes, int offset, DateTimeOffset value, IFormatterResolver formatterResolver)
        {
            var unixTimestamp = value.GetUnixTimestamp().TotalSeconds;

            return MessagePackBinary.WriteDouble(ref bytes, offset, unixTimestamp);
        }

        /// <inheritdoc cref="IMessagePackFormatter.Deserialize(byte[], int, IFormatterResolver, out int)" />
        public DateTimeOffset Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.GetMessagePackType(bytes, offset) == MessagePackType.Extension)
            {
                return DateTimeFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out readSize);
            }

            var unixTimestamp = MessagePackBinary.ReadInt64(bytes, offset, out readSize);

            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
        }
    }
}
