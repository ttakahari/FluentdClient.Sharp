using System.Collections.Generic;

namespace FluentdClient.Sharp.Tests
{
    public class TestSerializer : IMessagePackSerializer
    {
        public byte[] Serialize(string tag, IDictionary<string, object> message)
        {
            return new byte[0];
        }
    }
}
