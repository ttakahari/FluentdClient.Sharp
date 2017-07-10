using System;
using System.Threading.Tasks;
using Xunit;

namespace FluentdClient.Sharp.Tests
{
    public class FluentdClientTests
    {
        private const string Host = "localhost";
        private const int Port    = 24224;
        private readonly IMessagePackSerializer _serializer = new TestSerializer();

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

        // SendAsync_Tests in each serializers tests.

    }
}
