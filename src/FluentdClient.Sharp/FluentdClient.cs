using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FluentdClient.Sharp
{
    /// <summary>
    /// The class that connects to fluentd server.
    /// </summary>
    public class FluentdClient : IFluentdClient
    {
        private bool _disposed;
        private NetworkStream _stream;

        private readonly TcpClient _tcp;
        private readonly FluentdSetting _setting;

        /// <summary>
        /// Create a new <see cref="FluentdClient"/> instance.
        /// </summary>
        /// <param name="host">The host of fluentd server.</param>
        /// <param name="port">The port of fluentd server.</param>
        /// <param name="serializer">The MessagePack serializer.</param>
        public FluentdClient(string host, int port, IMessagePackSerializer serializer)
            : this(new FluentdSetting(host, port, serializer))
        { }

        /// <summary>
        /// Create a new <see cref="FluentdClient"/> instance.
        /// </summary>
        /// <param name="setting">The setting for connecting to fluentd server.</param>
        public FluentdClient(FluentdSetting setting)
        {
            if (setting == null)
            {
                throw new ArgumentNullException(nameof(setting));
            }

            _tcp     = new TcpClient();
            _setting = setting;
        }

        /// <inheritdoc cref="IFluentdClient.ConnectAsync" />
        public async Task ConnectAsync()
        {
            if (_setting.Timeout.HasValue)
            {
                _tcp.SendTimeout = _setting.Timeout.Value;
            }

            try
            {
                await _tcp.ConnectAsync(_setting.Host, _setting.Port).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (_setting.ExceptionHandler == null)
                {
                    throw;
                }

                _setting.ExceptionHandler.Invoke(ex);
            }
        }

        /// <inheritdoc cref="IFluentdClient.SendAsync{T}(string, T)" />
        public async Task SendAsync<T>(string tag, T message) where T : class
        {
            if (!(message is IDictionary<string, object> dictionary))
            {
                dictionary = TypeAccessor.GetMessageAsDictionary(message);
            }

            var value = _setting.Serializer.Serialize(tag, dictionary);

            try
            {
                if (!_tcp.Connected)
                {
                    await ConnectAsync().ConfigureAwait(false);
                }

                _stream = _tcp.GetStream();

                await _stream.WriteAsync(value, 0, value.Length).ConfigureAwait(false);
                await _stream.FlushAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (_setting.ExceptionHandler == null)
                {
                    throw;
                }

                _setting.ExceptionHandler.Invoke(ex);
            }
        }

        /// <summary>
        /// Destructor for not calling <see cref="Dispose()"/> method.
        /// </summary>
        ~FluentdClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Free, release, or reset managed or unmanaged resources.
        /// </summary>
        /// <param name="disposing">Wether to free, release, or resetting unmanaged resources or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _stream?.Dispose();
                _tcp?.Dispose();
            }

            _disposed = true;
        }
    }
}
