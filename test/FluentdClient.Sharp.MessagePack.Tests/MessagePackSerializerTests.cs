using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FluentdClient.Sharp.MessagePack.Tests
{
    public class MessagePackSerializerTests
    {
        [Fact]
        public void Constructor_Tests()
        {
            Assert.Throws<ArgumentNullException>(() => new MessagePackSerializer(null));

            {
                var serializer = new MessagePackSerializer();

                Assert.NotNull(serializer);
            }

            {
                var serializer = new MessagePackSerializer(CompositeResolver.Instance);

                Assert.NotNull(serializer);
            }
        }

        [Fact]
        public async Task Serialize_Tests()
        {
            using (var client = new FluentdClient("localhost", 24224, new MessagePackSerializer()))
            {
                await client.SendAsync("test.messagepack.simple", "hello fluentd by messagepack.");

                await client.SendAsync(
                    "test.messagepack.structured.class",
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
                    "test.messagepack.structured.dictionary",
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
                    "test.messagepack.structured.anonymous",
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
}
