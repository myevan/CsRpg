using Microsoft.EntityFrameworkCore;
using RpgServer.Databases;
using RpgServer.Serializers;

namespace RpgServer.Extensions
{
    public static class ServiceExtension
    {
        public static bool AddDistributedCacheUri(this IServiceCollection services, string uriStr)
        {
            var uri = new Uri(uriStr);
            if (uri.LocalPath.EndsWith(".msgpack"))
            {
                DistributedCacheExtension.SetPrefix("m.");
                services.AddSingleton<ICacheSerializer, MsgPackCacheSerializer>();
            }
            else
            {
                DistributedCacheExtension.SetPrefix("j.");
                services.AddSingleton<ICacheSerializer, JsonCacheSerializer>();
            }

            switch (uri.Scheme)
            {
                case "redis":
                    services.AddStackExchangeRedisCache(options => 
                    {
                        options.Configuration = $"{uri.Host}:{uri.Port}"; 
                    });
                    return true;
                case "memory":
                    services.AddDistributedMemoryCache();
                    return true;
                default:
                    return false;
            }
        }

        public static bool AddDbContextUri<T>(this IServiceCollection services, string uriStr)
        {
            var uri = new Uri(uriStr);
            switch (uri.Scheme)
            {
                case "memory":
                    services.AddDbContext<AuthDatabase>(options =>
                    {
                        options.UseInMemoryDatabase(uri.Host);
                    });
                    return true;
                default:
                    return false;
            }
        }
    }
}
