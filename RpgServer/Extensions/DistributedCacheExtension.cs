using Microsoft.Extensions.Caching.Distributed;
using RpgServer.ConstStrs;

namespace RpgServer.Extensions
{
    public static class DistributedCacheExtension
    {
        public static string GenIdfvKey(this IDistributedCache cache, string inIdfv)
        {
            if (string.IsNullOrEmpty(inIdfv)) { return string.Empty; }

            return $"{_prefix}{FourccStr.Idfv}:{inIdfv}";
        }

        public static string GenAccountKey(this IDistributedCache cache, ulong inAccountId)
        {
            if (inAccountId == 0) { return string.Empty; }

            return $"{_prefix}{FourccStr.Account}:{inAccountId}";
        }

        public static string GenSessionKey(this IDistributedCache cache, string inSessionId)
        {
            if (string.IsNullOrEmpty(inSessionId)) { return string.Empty; }

            return $"{_prefix}{FourccStr.Session}:{inSessionId}";
        }

        public static void SetPrefix(string prefix) { _prefix = prefix; }

        private static string _prefix = string.Empty;
    }
}
