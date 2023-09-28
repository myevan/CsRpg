using Microsoft.EntityFrameworkCore;
using RpgServer.DbContexts;

namespace RpgServer.Extensions
{
    public static class ServiceExtension
    {
        public static bool AddDistributedCacheUri(this IServiceCollection services, string uriStr)
        {
            var uri = new Uri(uriStr);
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
