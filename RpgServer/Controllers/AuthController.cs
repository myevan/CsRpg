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
                Ctx = _ctx.Payload
            };
        }

        [Route("/auth/sign-up")]
        [HttpPost]
        public object SignUp(string idfv)
        {
            _ctx.SetLogSub(11);

            var oldDevice = _repo.LoadDevice(idfv);
            if (oldDevice == null) // �ű� ����̽����
            {
                // �ű� ��ī��Ʈ-����̽�-���� ����
                var newAccount = _repo.GenAccount(idfv);
                var newDevice = _repo.GenDevice(idfv, newAccount.Id);
                var newSession = _repo.GenSession(newAccount, newDevice);
                _ctx.Authorize(newAccount, newDevice, newSession);

                return new
                {
                    Msg = "NEW_ACCOUNT",
                    Ctx = _ctx.Payload
                };
            }
            else // ���� ����̽����
            {
                // ���� ��ī��Ʈ �ҷ���
                var oldAccount = _repo.LoadAccount(oldDevice.AccountId);
                if (oldAccount == null) // ���� ��ī��Ʈ�� ���ٸ�
                {
                    // TODO: ���� ��ī��Ʈ ����?
                    throw new Exception($"NOT_FOUND_ACCOUNT({oldDevice.AccountId}) IDFV({oldDevice.Idfv})");
                }
                else // ���� ��ī��Ʈ�� �ִٸ�
                {
                    // ���� ���� �ҷ�����
                    var oldSession = _repo.TouchSession(oldAccount, oldDevice);

                    // ���� ���̵� ����
                    _repo.ResetSessionId(oldSession, oldAccount, oldDevice);

                    _ctx.Authorize(oldAccount, oldDevice, oldSession);

                    return new
                    {
                        Msg = "OLD_ACCOUNT",
                        Ctx = _ctx.Payload
                    };
                }
            }
        }

        private readonly ContextService _ctx;
        private readonly AuthRepository _repo;
    }
}