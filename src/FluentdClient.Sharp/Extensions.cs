using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FluentdClient.Sharp.MsgPack")]
[assembly: InternalsVisibleTo("FluentdClient.Sharp.MessagePack")]
namespace FluentdClient.Sharp
{
    internal static class Extensions
    {
        private static readonly DateTime _epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTimeOffset _epochDateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        internal static TimeSpan GetUnixTimestamp(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().Subtract(_epochDateTime);
        }

        internal static TimeSpan GetUnixTimestamp(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToUniversalTime().Subtract(_epochDateTimeOffset);
        }

        internal static bool IsAnonymous(this TypeInfo type)
        {
            var hasAttribute = type.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null;
            var containsName = type.FullName.Contains("AnonymousType");

            return hasAttribute && containsName;
        }
    }
}
