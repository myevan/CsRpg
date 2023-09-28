using Microsoft.AspNetCore.Mvc;
using RpgServer.Repositories;
using RpgServer.Services;

namespace RpgServer.Controllers
{
    
    [ApiController]
    public class AuthController : Controller
    {
        public AuthController(ContextService ctx, AuthRepository repo)
        {
            _ctx = ctx;
            _repo = repo;
            
            _ctx.SetLogMain(10);
        }

        [Route("/auth")]
        [HttpGet]
        public object GetInfo(string sessionId)
        {
            if (!_ctx.IsAuthorized())
            {
                return new
                {
                    Msg = "NOT_AUTHORIZED",
                    Ctx = _ctx.Payload
                };
            }

            return new
            {
                Ctx = _ctx.Payload
            };
        }

        [Route("/auth/sign-up")]
        [HttpPost]
        public object SignUp(string idfv)
        {
            _ctx.SetLogSub(11);

            if (!_ctx.TryAuthorizeByIdfv(idfv))
            {
                return new
                {
                    Msg = "OLD_ACCOUNT",
                    Ctx = _ctx.Payload
                };
            }

            return new
            {
                Msg = "NEW_ACCOUNT",
                Ctx = _ctx.Payload
            };
        }

        private readonly ContextService _ctx;
        private readonly AuthRepository _repo;
    }
}