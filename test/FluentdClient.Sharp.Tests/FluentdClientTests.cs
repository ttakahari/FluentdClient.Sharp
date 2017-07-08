using FluentdClient.Sharp.MsgPackCli;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FluentdClient.Sharp.Tests
{
    public class FluentdClientTests
{
        private const string Host = "localhost";
        private const int Port    = 24224;
        private readonly IMessagePackSerializer _serializer = new MsgPackCliSerializer();

        [Fact]
        public void Constructor_Tests()
        {
            Assert.Throws<ArgumentNullException>(() => new FluentdClient("", Port, _serializer));
            Assert.Throws<ArgumentNullException>(() => new FluentdClient(Host, Port, null));

            using (var client = new FluentdClient(Host, Port, _serializer))
            {
                Assert.NotNull(client);
            }

            Assert.Throws<ArgumentNullException>(() => new FluentdClient(null));

            using (var client = new FluentdClient(new FluentdSetting(Host, Port, _serializer)))
            {
                Assert.NotNull(client);
            }
        }

        [Fact]
        public async Task ConnectAsync_Tests()
        {
            using (var client = new FluentdClient(new FluentdSetting("172.0.0.1", Port, _serializer) { Timeout = 1000, ExceptionHandler = ex => { Assert.NotNull(ex); } }))
            {
                await client.ConnectAsync();
            }

            using (var client = new FluentdClient(Host, Port, _serializer))
            {
                await client.ConnectAsync();
            }
        }

        [Fact]
        public async Task SendAsync_Test()
        {
            using (var client = new FluentdClient(Host, Port, _serializer))
            {
                // Dictionary
                {
                    var message = new Dictionary<string, object>
                    {
                        { "Id"          , 100 },
                        { "Name"        , "AAA" },
                        { "Timestamp"   , DateTimeOffset.Now },
                        { "IsAnonymous" , false },
                        { "IsDictionary", true }
                    };

                    await client.SendAsync("test.aaa", message);
                }

                // Anonymous
                {
                    var message = new
                    {
                        Id           = 200,
                        Name         = "BBB",
                        Timestamp    = DateTimeOffset.Now,
                        IsAnonymous  = true,
                        IsDictionary = false
                    };

                    await client.SendAsync("test.bbb", message);
                }

                // Class
                {
                    var message = new Payload
                    {
                        Id           = 300,
                        Name         = "CCC",
                        Timestamp    = DateTimeOffset.Now,
                        IsAnonymous  = false,
                        IsDictionary = false
                    };

                    await client.SendAsync("test.ccc", message);
                }
            }
        }

        private class Payload
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public DateTimeOffset Timestamp { get; set; }

            public bool IsAnonymous { get; set; }

            public bool IsDictionary { get; set; }
        }
    }
}
