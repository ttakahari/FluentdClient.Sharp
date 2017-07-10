using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FluentdClient.Sharp.MessagePack.Tests
{
    public class FluentdClientTests
    {
        [Fact]
        public async Task SendAsync_Tests()
        {
            CompositeResolver.RegisterAndSetAsDefault(new[]
            {
                UnixTimestampResolver.Instance,
                StandardResolver.Instance
            });

            using (var client = new FluentdClient("localhost", 24224, new MessagePackSerializer(CompositeResolver.Instance)))
            {
                await client.SendAsync(
                    "test.aaa",
                    new Message
                    {
                        Id        = 1,
                        Name      = "AAA",
                        Timestamp = DateTimeOffset.Now,
                        IsMessage = true,
                        Type      = MessageType.Class,
                        Array     = new object[] { 1, "XXX", true, 10.5D }
                    });

                await client.SendAsync(
                    "test.bbb",
                    new Dictionary<string, object>
                    {
                        { "Id"       , 2 },
                        { "Name"     , "BBB" },
                        { "Timestamp", DateTimeOffset.Now },
                        { "IsMessage", true },
                        { "Type"     , MessageType.Dictionary },
                        { "Nested"   , new Message { Id = 1, Name = "AAA", Timestamp = DateTimeOffset.Now, IsMessage = true, Type = MessageType.Class } }
                    });

                await client.SendAsync(
                    "test.ccc",
                    new
                    {
                        Id         = 3,
                        Name       = "CCC",
                        Timestamp  = DateTimeOffset.Now,
                        IsMessage  = true,
                        Type       = MessageType.Anonymous,
                        Dictionary = new Dictionary<string, object> { { "Id", 3 }, { "Name", "CCC" }, { "Array", new object[] { 1, "A", true } } }
                    });
            }
        }
    }

    [MessagePackObject]
    public class Message
    {
        [Key(0)]
        public int Id { get; set; }

        [Key(1)]
        public string Name { get; set; }

        [Key(2)]
        public DateTimeOffset Timestamp { get; set; }

        [Key(3)]
        public bool IsMessage { get; set; }

        [Key(4)]
        public MessageType Type { get; set; }

        [Key(5)]
        public object[] Array { get; set; }
    }

    public enum MessageType
    {
        Class      = 1,
        Dictionary = 2,
        Anonymous  = 3
    }

    #region UnixTimestampResolver

    public sealed class UnixTimestampResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new UnixTimestampResolver();

        UnixTimestampResolver()
        {

        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                formatter = (IMessagePackFormatter<T>)UnixTimestampResolverGetFormatterHelper.GetFormatter(typeof(T));
            }
        }
    }

    internal static class UnixTimestampResolverGetFormatterHelper
    {
        internal static object GetFormatter(Type t)
        {
            if (t == typeof(DateTime))
            {
                return UnixTimestampDateTimeFormatter.Instance;
            }
            else if (t == typeof(DateTime?))
            {
                return new StaticNullableFormatter<DateTime>(UnixTimestampDateTimeFormatter.Instance);
            }
            else if (t == typeof(DateTime[]))
            {
                return new ArrayFormatter<DateTime>(); // これが必要なのはMessagePack for C#ヨクナインデスケドネ（要修正）
            }
            else if (t == typeof(DateTimeOffset))
            {
                return UnixTimestampDateTimeOffsetFormatter.Instance;
            }
            else if (t == typeof(DateTimeOffset?))
            {
                return new StaticNullableFormatter<DateTimeOffset>(UnixTimestampDateTimeOffsetFormatter.Instance);
            }

            return null;
        }
    }

    public sealed class UnixTimestampDateTimeFormatter : IMessagePackFormatter<DateTime>
    {
        public static readonly UnixTimestampDateTimeFormatter Instance = new UnixTimestampDateTimeFormatter();

        UnixTimestampDateTimeFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, DateTime value, IFormatterResolver formatterResolver)
        {

            var dateData = ((DateTimeOffset)value).ToUnixTimeSeconds();
            return MessagePackBinary.WriteInt64(ref bytes, offset, dateData);
        }

        public DateTime Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.GetMessagePackType(bytes, offset) == MessagePackType.Extension)
            {
                return DateTimeFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out readSize);
            }

            var dateData = MessagePackBinary.ReadInt64(bytes, offset, out readSize);

            return DateTimeOffset.FromUnixTimeSeconds(dateData).UtcDateTime;
        }
    }

    public sealed class UnixTimestampDateTimeOffsetFormatter : IMessagePackFormatter<DateTimeOffset>
    {
        public static readonly UnixTimestampDateTimeOffsetFormatter Instance = new UnixTimestampDateTimeOffsetFormatter();

        UnixTimestampDateTimeOffsetFormatter()
        {

        }

        public int Serialize(ref byte[] bytes, int offset, DateTimeOffset value, IFormatterResolver formatterResolver)
        {

            var dateData = value.ToUnixTimeSeconds();
            return MessagePackBinary.WriteInt64(ref bytes, offset, dateData);
        }

        public DateTimeOffset Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.GetMessagePackType(bytes, offset) == MessagePackType.Extension)
            {
                return DateTimeFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out readSize);
            }

            var dateData = MessagePackBinary.ReadInt64(bytes, offset, out readSize);

            return DateTimeOffset.FromUnixTimeSeconds(dateData);
        }
    }

    #endregion
}
