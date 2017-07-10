using System;
using Xunit;

namespace FluentdClient.Sharp.Tests
{
    public class FluentdSettingTests
    {
        [Fact]
        public void Tests()
        {
            const string host = "localhost";
            const int port    = 24224;
            const int timeout = 1000;

            var serializer      = new TestSerializer();
            var exceptonHandler = new Action<Exception>(ex => { });

            Assert.Throws<ArgumentNullException>(() => new FluentdSetting("", port, serializer));
            Assert.Throws<ArgumentNullException>(() => new FluentdSetting(host, 0, null));

            var setting = new FluentdSetting(host, port, serializer);

            Assert.NotNull(setting);
            Assert.Equal(setting.Host, host);
            Assert.Equal(setting.Port, port);
            Assert.Equal(setting.Serializer.GetType(), serializer.GetType());
            Assert.False(setting.Timeout.HasValue);
            Assert.Null(setting.ExceptionHandler);

            setting.Timeout          = timeout;
            setting.ExceptionHandler = exceptonHandler;

            Assert.Equal(setting.Timeout, timeout);
            Assert.NotNull(setting.ExceptionHandler);
        }
    }
}
