using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
#if NETSTANDARD1_6
using System.Reflection;
#endif

namespace FluentdClient.Sharp
{
    internal static class TypeAccessor
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>> _valueGetter;

        static TypeAccessor()
        {
            _valueGetter = new ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>>();
        }

        internal static IDictionary<string, object> GetMessageAsDictionary<T>(T message) where T : class
        {
            var valueType = message.GetType();

            var valueGetter = _valueGetter.GetOrAdd(
                valueType,
                type =>
                {
                    var properties = type.GetProperties();

                    return properties.ToDictionary(
                        x => x.Name,
                        x => new Func<object, object>(obj => x.GetValue(obj)));
                });

            var dictionary = valueGetter.ToDictionary(x => x.Key, x => x.Value.Invoke(message));

            return dictionary;
        }

        internal static IDictionary<string, Func<object, object>> GetValueGetter(Type type)
        {
            return _valueGetter.GetOrAdd(
                type,
                valueType =>
                {
                    var properties = valueType.GetProperties();

                    return properties.ToDictionary(
                        x => x.Name,
                        x => new Func<object, object>(obj => x.GetValue(obj)));
                });
        }
    }
}
