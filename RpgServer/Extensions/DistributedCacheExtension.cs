using Microsoft.Extensions.Caching.Distributed;
using RpgServer.ConstStrs;

namespace RpgServer.Extensions
{
    public static class DistributedCacheExtension
    {
        public static string GenIdfvKey(this IDistributedCache cache, string inIdfv)
        {
            if (string.IsNullOrEmpty(inIdfv)) { return string.Empty; }

            return $"{FourccStr.Idfv}:{inIdfv}";
        }

        public static string GenAccountKey(this IDistributedCache cache, ulong inAccountId)
        {
            if (inAccountId == 0) { return string.Empty; }

            return $"{FourccStr.Account}:{inAccountId}";
        }

        public static string GenSessionKey(this IDistributedCache cache, string inSession)
        {
            if (string.IsNullOrEmpty(inSession)) { return string.Empty; }

            return $"{FourccStr.Session}:{inSession}";
        }

    }
}
