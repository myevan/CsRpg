using Microsoft.AspNetCore.Mvc;
using RpgServer.Repositories;
using RpgServer.Services;
using System.Security.Principal;

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
                Msg = "AUTHORIZED",
                Ctx = _ctx.Payload
            };
        }

        [Route("/auth")]
        [HttpPost]
        public object GenAuth(string idfv)
        {
            _ctx.SetLogSub(11);

            var oldDevice = _repo.LoadDevice(idfv);
            if (oldDevice == null) // 신규 디바이스라면
            {
                // 신규 어카운트-디바이스-세션 생성
                var newAccount = _repo.GenAccount(idfv);
                var newDevice = _repo.GenDevice(idfv, newAccount.Id);
                var newSession = _repo.GenSession(newAccount, newDevice);
                _ctx.Authorize(newAccount, newDevice, newSession);

                return new
                {
                    Msg = "NEW_DEVICE_ACCOUNT",
                    Ctx = _ctx.Payload
                };
            }
            else // 기존 디바이스라면
            {
                // 기존 어카운트 불러옴
                var oldAccount = _repo.LoadAccount(oldDevice.AccountId);
                if (oldAccount == null) // 기존 어카운트가 없다면
                {
                    // TODO: 기존 어카운트 삭제?
                    throw new Exception($"NOT_FOUND_ACCOUNT({oldDevice.AccountId}) IDFV({oldDevice.Idfv})");
                }
                else // 기존 어카운트가 있다면
                {
                    // 기존 세션 불러오기
                    var oldSession = _repo.TouchSession(oldAccount, oldDevice);

                    // 세션 아이디 리셋
                    if (!_repo.TryResetSessionId(oldSession, oldAccount, oldDevice))
                    {
                        // 어카운트 세션 리셋 불가 (캐시-데이터베이스 동기화 불일치)
                        return new
                        {
                            Msg = "NOT_RESET_ACCOUNT_SESSION",
                            Ctx = _ctx.Payload
                        };
                    }

                    // 컨텍스트 인증
                    _ctx.Authorize(oldAccount, oldDevice, oldSession);

                    return new
                    {
                        Msg = "FOUND_DEVICE_ACCOUNT",
                        Ctx = _ctx.Payload
                    };
                }
            }
        }

        private readonly ContextService _ctx;
        private readonly AuthRepository _repo;
    }
}