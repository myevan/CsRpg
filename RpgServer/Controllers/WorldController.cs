using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RpgServer.Configs;
using RpgServer.Databases;
using RpgServer.Services;

namespace RpgServer.Controllers
{
    [ApiController]
    public class WorldController
    {
        public WorldController(ContextService ctx)
        {
            _ctx = ctx;
            _ctx.SetLogMain(0);
        }

        [Route("/world")]
        [HttpGet]
        public object GetWorld()
        {
            return new
            {
                Ctx = _ctx.Payload,
            };
        }

        private ContextService _ctx;
    }
}
