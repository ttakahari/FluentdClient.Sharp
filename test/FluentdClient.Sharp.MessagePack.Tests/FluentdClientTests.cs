using MessagePack;
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
                PayloadFormtterResolver.Instance,
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
                        // can't serialize nested object types.
                        //{ "Nested"   , new Message { Id = 1, Name = "AAA", Timestamp = DateTimeOffset.Now, IsMessage = true, Type = MessageType.Class } }
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
}
