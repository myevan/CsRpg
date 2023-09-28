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

        public bool TryAuthorizeByIdfv(string inIdfv)
        {
            _isUpdated = false;

            var oldDevice = _authRepo.LoadDevice(inIdfv);
            if (oldDevice == null) // 신규 디바이스라면
            {
                // 신규 어카운트-디바이스-세션 생성
                var newAccount = _authRepo.GenAccount(inIdfv);
                var newDevice = _authRepo.GenDevice(inIdfv, newAccount.Id);
                var newSession = _authRepo.GenSession(newAccount, newDevice);
                _slotSession.Set(newSession);
                _slotAccount.Set(newAccount);
                _slotDevice.Set(newDevice);

                _slotSessionId.Set(newAccount.SessionId);

                return true;
            }
            else // 기존 디바이스라면
            {
                // 기존 어카운트 불러옴
                var oldAccount = _authRepo.LoadAccount(oldDevice.AccountId);
                if (oldAccount == null) // 기존 어카운트가 없다면
                {
                    // TODO: 기존 어카운트 삭제?
                    throw new Exception($"NOT_FOUND_ACCOUNT({oldDevice.AccountId}) IDFV({oldDevice.Idfv})");
                }
                else // 기존 어카운트가 있다면
                {
                    // 기존 세션 불러오기
                    var oldSession = _authRepo.TouchSession(oldAccount, oldDevice);

                    // 세션 아이디 리셋
                    _authRepo.ResetSessionId(oldSession, oldAccount, oldDevice);

                    _slotSession.Set(oldSession);
                    _slotAccount.Set(oldAccount);
                    _slotDevice.Set(oldDevice);

                    _slotSessionId.Set(oldAccount.SessionId);
                    return false;
                }
            }
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
