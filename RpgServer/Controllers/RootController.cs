using Microsoft.AspNetCore.Mvc;

namespace RpgServer.Controllers
{
    using Services;

    [Route("/")]
    [ApiController]
    public class RootController : Controller
    {
        public RootController(ContextService ctxSvc)
        {
            _ctx = ctxSvc;
            _ctx.SetMain(10);
        }

        [Route("/")]
        [HttpGet]
        public object GetRoot()
        {
            return new
            {
                Message = "Hello"
            };
        }

        [Route("/example/auth")]
        [HttpPost]
        public object GenExampleAuth()
        {
            _ctx.SetSub(11);

            _ctx.GenSessionId();
            _ctx.SetAccountId(100);
            _ctx.SaveSessionCache();

            _ctx.LockAccount();

            return new
            {
                Context = _ctx.Payload
            };
        }

        [Route("/example/auth")]
        [HttpGet]
        public object GetExampleAuth(string sessionId)
        {
            _ctx.SetSub(12);

            _ctx.LoadSessionCache(sessionId);

            if (!_ctx.TryLockAccount())
            {
                return new
                {
                    Error = "ACCOUNT_LOCKED"
                };
            }

            return new
            {
                Context = _ctx.Payload
            };
        }

        private ContextService _ctx;
    }
}