using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RpgServer.Configs;
using RpgServer.Databases;
using RpgServer.Services;

namespace RpgServer.Controllers
{
    
    [Route("/")]
    [ApiController]
    public class RootController : Controller
    {
        public RootController(IDistributedCache cache, AuthDatabase authDb, ContextConfig cfg, ContextService ctx)
        {
            _cache = cache;
            _authDb = authDb;
            _cfg = cfg;
            _ctx = ctx;
            _ctx.SetLogMain(0);
        }

        [Route("/")]
        [HttpGet]
        public object GetHello()
        {
            var distCacheAppKey = $"{_cfg.Project}.{_cfg.App}";
            _cache.SetString(distCacheAppKey, _cfg.Rev);
            var distCachedRev = _cache.GetString(distCacheAppKey);
            var distCacheState = distCachedRev == _cfg.Rev ? $"{distCacheAppKey}={distCachedRev}" : "ERROR";
            var dbAccountCount = _authDb.AccountSet.Count();

            return new
            {
                Msg = "Hello",
                DistCacheState = distCacheState,
                DbAccountCount = dbAccountCount,
                Ctx = _ctx.Payload,
            };
        }

        private IDistributedCache _cache;
        private AuthDatabase _authDb;
        private ContextConfig _cfg;
        private ContextService _ctx;

    }
}