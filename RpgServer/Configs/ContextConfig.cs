using Microsoft.Extensions.Caching.Distributed;

namespace RpgServer.Configs
{
    public class ContextConfig
    {
        public string Project { get; private set; } = "RPG";
        public string Region { get; private set; } = "";
        public string App { get; private set; } = "WAS";
        public string Rev { get; private set; } = "${REV}";

        public DistributedCacheEntryOptions SessionCacheOpts = new DistributedCacheEntryOptions
        {
            // NOTE: 요청시마다 시간 연장 SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
        };

        public DistributedCacheEntryOptions AccountCacheOpts = new DistributedCacheEntryOptions
        {
            // NOTE: 요청시마다 시간 연장 SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
        };

        public DistributedCacheEntryOptions DeviceCacheOpts = new DistributedCacheEntryOptions
        {
            // NOTE: 요청시마다 시간 연장 SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
        };


    }
}
