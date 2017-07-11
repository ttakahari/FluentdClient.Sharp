using System;
using System.Collections.Generic;

namespace FluentdClient.Sharp.Tests
{
    public class TestSerializer : IMessagePackSerializer
    {
        public byte[] Serialize(string tag, string message)
        {
            return new byte[0];
        }

        public byte[] Serialize<T>(string tag, T message) where T : class
        {
            return new byte[0];
        }
    }
}
