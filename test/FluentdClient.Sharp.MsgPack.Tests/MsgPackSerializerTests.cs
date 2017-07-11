using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FluentdClient.Sharp.MsgPack.Tests
{
    public class MsgPackSerializerTests
    {
        [Fact]
        public void Constructor_Tests()
        {
            Assert.Throws<ArgumentNullException>(() => new MsgPackSerializer(null));

            {
                var serializer = new MsgPackSerializer();

                Assert.NotNull(serializer);
            }

            {
                var serializer = new MsgPackSerializer(SerializationContext.Default);

                Assert.NotNull(serializer);
            }
        }

        [Fact]
        public async Task Serialize_Tests()
        {
            using (var client = new FluentdClient("localhost", 24224, new MsgPackSerializer()))
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

    public class Message
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public bool IsMessage { get; set; }

        public MessageType Type { get; set; }

        public object[] Array { get; set; }
    }

    public enum MessageType
    {
        Class      = 1,
        Dictionary = 2,
        Anonymous  = 3
    }
}
