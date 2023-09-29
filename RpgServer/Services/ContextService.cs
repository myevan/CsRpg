using RpgServer.Configs;
using RpgServer.Collections;
using RpgServer.Models;
using RpgServer.Repositories;
using RpgServer.Extensions;
using System.Security.Principal;

namespace RpgServer.Services
{
    using Payload = Dictionary<string, object>;

    /// <summary>
    /// 컨텍스트 서비스
    /// 
    /// HTTP 요청 -> 쿼리(세션 아이디, 시퀀스), 헤더(세션 아이디, 유저 에이전트)
    /// 세션 로드 -> 캐쉬(어카운트 아이디, 플레이어 아이디, 디바이스 아이디), 만료 시간 업데이트
    /// 로그 페이로드 -> 제품 정보(상수, 설정) + 유저 정보(세션) + 요청 정보(시퀀스)
    /// </summary>
    public class ContextService
    {
        public ContextService(IHttpContextAccessor accessor, AuthRepository authRepo, ContextConfig config)
        {
            _accessor = accessor;
            _authRepo = authRepo;
            _config = config;
        }

        public ContextConfig Config { get { return _config; } }

        public string SessionId { get { return _slotSessionId.Get(LoadRequestSessionId); } }
        public ulong Seq { get { return _slotSeq.Get(LoadRequestSeq); } }

        public SessionModel Session { get { return _slotSession.Get(LoadSessionModel); } }

        public AccountModel Account { get { return _slotAccount.Get(LoadAccountModel); } }

        public DeviceModel Device { get { return _slotDevice.Get(LoadDeviceModel); } }

        public Payload Payload { get { return UpdatePayload(); } }

        public void SetLogMain(int value)
        {
            _logMain = value;
            _payload["Main"] = value;
            _payload["Sub"] = 0;
        }

        public void SetLogSub(int value)
        {
            _logSub = value;
            _payload["Sub"] = value;
        }

        public bool IsAuthorized()
        {
            var session = this.Session;
            return session.AccountId != 0;
        }

        public void Authorize(AccountModel account, DeviceModel device, SessionModel session)
        {
            _isUpdated = false;

            _slotAccount.Set(account);
            _slotDevice.Set(device);

            _slotSession.Set(session);
            _slotSessionId.Set(account.SessionId);
        }

        private string LoadRequestSessionId()
        {
            return GetRequestQueryVar("SessionId");
        }
        private ulong LoadRequestSeq()
        {
            return GetRequestQueryVar("Seq").ToUInt64();
        }

        private SessionModel? LoadSessionModel()
        {
            return _authRepo.LoadSession(SessionId);
        }

        private AccountModel? LoadAccountModel()
        {
            return _authRepo.LoadAccount(Session.AccountId);
        }

        private DeviceModel? LoadDeviceModel()
        {
            return _authRepo.LoadDevice(Account.Idfv);
        }

        /// <summary>
        /// 페이로드 업데이트
        /// </summary>
        /// <returns>페이로드</returns>
        public Payload UpdatePayload()
        {
            if (_isUpdated) { return _payload; }

            _isUpdated = true;

            var seq = this.Seq;
            var sessionId = this.SessionId;
            var session = this.Session;
            var config = this.Config;

            _payload["Seq"] = seq;
            _payload["SessionId"] = sessionId;

            _payload["PlayerId"] = session.PlayerId;
            _payload["AccountId"] = session.AccountId;

            _payload["Idfv"] = session.Idfv;
            _payload["Idfa"] = session.Idfa;

            _payload["Project"] = config.Project;
            _payload["Region"] = config.Region;
            _payload["App"] = config.App;
            _payload["Rev"] = config.Rev;

            _payload["Main"] = _logMain;
            _payload["Sub"] = _logSub;

            return _payload;
        }

        private string GetRequestQueryVar(string key)
        {
            var httpCtx = _accessor.HttpContext;
            if (httpCtx != null)
            {
                if (httpCtx.Request.Query.TryGetValue(key, out var val))
                {
                    return val.ToString();
                }
            }

            return string.Empty;
        }


        private readonly IHttpContextAccessor _accessor;

        private readonly AuthRepository _authRepo;

        private readonly ContextConfig _config;
        
        private Slot<SessionModel> _slotSession = new(new SessionModel());
        private Slot<AccountModel> _slotAccount = new(new AccountModel());
        private Slot<DeviceModel> _slotDevice = new(new DeviceModel());

        private Slot<string> _slotSessionId = new(string.Empty);
        private Slot<ulong> _slotSeq = new(0);

        private Payload _payload = new Payload();

        private int _logMain = 0;
        private int _logSub = 0;

        private bool _isUpdated = false;
    }
}
