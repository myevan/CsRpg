using Microsoft.Extensions.Caching.Distributed;
using RpgServer.ConstStrs;
using RpgServer.Models;
using RpgServer.Serializers;

namespace RpgServer.Extensions
{
    public static class DistributedCacheExtension
    {
        public static void SetDevice(this IDistributedCache cache, ICacheSerializer serializer, DeviceModel model, DistributedCacheEntryOptions options)
        {
            if (string.IsNullOrEmpty(model.Idfv)) return;
            
            var key = cache.GenDeviceKey(model.Idfv);
            var bytes = serializer.Dump(model);
            cache.Set(key, bytes, options);
        }

        public static void SetAccount(this IDistributedCache cache, ICacheSerializer serializer, AccountModel model, DistributedCacheEntryOptions options)
        {
            if (model.Id == 0) return;
            
            var key = cache.GenAccountKey(model.Id);
            var bytes = serializer.Dump(model);
            cache.Set(key, bytes, options);
        }

        public static void SetSession(this IDistributedCache cache, ICacheSerializer serializer, SessionModel model, DistributedCacheEntryOptions options)
        {
            if (string.IsNullOrEmpty(model.Id)) return;
            
            var key = cache.GenSessionKey(model.Id);
            var bytes = serializer.Dump(model);
            cache.Set(key, bytes, options);
        }
        public static void RemoveAccount(this IDistributedCache cache, ulong id)
        {
            if (id == 0) return;

            var key = cache.GenAccountKey(id);
            cache.Remove(key);
        }

        public static void RemoveSession(this IDistributedCache cache, string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            var key = cache.GenSessionKey(id);
            cache.Remove(key);
        }

        public static string GenDeviceKey(this IDistributedCache cache, string id)
        {
            return $"{_prefix}{FourccStr.Idfv}:{id}";
        }

        public static string GenAccountKey(this IDistributedCache cache, ulong id)
        {
            return $"{_prefix}{FourccStr.Account}:{id}";
        }

        public static string GenSessionKey(this IDistributedCache cache, string id)
        {
            return $"{_prefix}{FourccStr.Session}:{id}";
        }

        public static void SetPrefix(string prefix) { _prefix = prefix; }

        private static string _prefix = string.Empty;
    }
}
