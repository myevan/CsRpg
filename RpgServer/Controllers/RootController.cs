using Microsoft.AspNetCore.Mvc;
using RpgServer.Services;

namespace RpgServer.Controllers
{
    
    [Route("/")]
    [ApiController]
    public class RootController : Controller
    {
        public RootController(ContextService ctxSvc)
        {
            _ctx = ctxSvc;
            _ctx.SetLogMain(0);
        }

        [Route("/")]
        [HttpGet]
        public object GetRoot()
        {
            return new
            {
                Msg = "Hello",
                Ctx = _ctx.Payload
            };
        }

        private ContextService _ctx;
    }
}