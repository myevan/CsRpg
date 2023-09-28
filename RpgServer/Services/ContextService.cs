using Microsoft.Extensions.Caching.Distributed;
using RpgServer.Interfaces;
using RpgServer.Contexts;
using RpgServer.Configs;

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
        
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="accessor">HTTP 컨텍스트 접근 인터페이스</param>
        /// <param name="cache">분산 캐시 인터페이스</param>
        /// <param name="serializer">캐시 시리얼라이저</param>
        public ContextService(IHttpContextAccessor accessor, IDistributedCache cache, ICacheSerializer serializer, ContextConfig config)
        {
            _accessor = accessor;
            _cache = cache;
            _serializer = serializer;
            _config = config;
        }

        public ContextConfig Config { get { return _config; } }

        public LoggingCache Logging { get { return _loggingCache; } }

        public RequestCache Request { get { return Cache(ref _isRequestCached, ref _requestCache, LoadRequestCache); } }

        public SessionCache Session { get { return Cache(ref _isSessionCached, ref _sessionCache, LoadSessionCache); } }
        
        public AccountCache Account { get { return Cache(ref _isAccountCached, ref _accountCache, LoadAccountCache); } }
        
        public Payload Payload { get { return Cache(ref _isPayloadCached, ref _payload, UpdatePayload); } }

        public void SetAccountId(ulong value) { _sessionCache.AccountId = value; }

        public void SetPlayerId(ulong value) { _sessionCache.PlayerId = value; }

        public void SetIdfv(string value) { _sessionCache.Idfv = value; }

        public void SetIdfa(string value) { _sessionCache.Idfa = value; }

        public void SetMain(int value) { _loggingCache.Main = value; }

        public void SetSub(int value) { _loggingCache.Sub = value; }

        /// <summary>
        /// 세션 아이디 생성
        /// </summary>
        public void GenSessionId()
        {
            var newGuid = Guid.NewGuid();
            _requestCache.SessionId = newGuid.ToString();
        }

        /// <summary>
        /// 세션 저장
        /// </summary>
        /// <returns>수행 결과</returns>
        public bool SaveSessionCache()
        {
            return SaveSessionCache(this.Request.SessionId);
        }

        public bool SaveSessionCache(string sessionId)
        {
            if (!TryGetSessionCacheKey(sessionId, out string key)) { return false; }

            var bytes = _serializer.Dump(_sessionCache);
            _cache.Set(key, bytes); // TODO: 만료 시간 설정
            return true;
        }

        /// <summary>
        /// 세션 캐시 제거
        /// </summary>
        /// <param name="sessionId">세션 아이디</param>
        /// <returns>수행 결과</returns>
        public bool RemoveSessionCache(string sessionId)
        {
            if (!TryGetSessionCacheKey(sessionId, out string key)) { return false; }

            _cache.Remove(key);
            return true;
        }

        /// <summary>
        /// 세션 불러오기 (요청시 한번만 로드하고 캐시 리턴)
        /// </summary>
        /// <returns>세션 컨텍스트</returns>
        public SessionCache LoadSessionCache()
        {
            return LoadSessionCache(this.Request.SessionId);
        }

        public SessionCache LoadSessionCache(string sessionId)
        {
            if (!TryGetSessionCacheKey(sessionId, out string key)) { return _sessionCache; }

            var bytes = _cache.Get(key);
            if (bytes != null)
            {
                var sessionCache = _serializer.Load<SessionCache>(bytes);
                if (sessionCache != null)
                {
                    // TODO: 만료 시간 갱신
                    _sessionCache = sessionCache;
                    return _sessionCache;
                }
            }

            // TODO: 분산 캐쉬 접근이 안 되면 임시로 데이터베이스에서 로드
            return _sessionCache;
        }

        /// <summary>
        /// 어카운트 잠금 (중복 로그인 방지)
        /// </summary>
        public bool LockAccount()
        {
            var sessionId = this.Request.SessionId;
            var accountId = this.Session.AccountId;
            return LockAccount(sessionId, accountId);
        }

        public bool LockAccount(string sessionId, ulong accountId)
        {
            // 어카운트에 연결된 세션 아이디가 동일한지 확인
            var accountCache = LoadAccountCache(sessionId, accountId);
            if (accountCache.SessionId != sessionId)
            {
                // 세션 아이디가 다르다면 중복 로그인 (혹은 분산 캐쉬 에러 상황)이므로 기존 세션 아이디 제거
                RemoveSessionCache(accountCache.SessionId);

                // TODO: 데이터베이스 확인

                // 새로운 세션 아이디 교체
                accountCache.SessionId = sessionId;

                // 어카운트 캐쉬 저장
                SaveAccountCache(accountId, accountCache);
            }
            return true;
        }

        /// <summary>
        /// 어카운트 잠금 확인 (중복 로그인 방지)
        /// </summary>
        public bool TryLockAccount()
        {
            var sessionId = this.Request.SessionId;
            var accountId = this.Session.AccountId;
            return TryLockAccount(sessionId, accountId);
        }

        public bool TryLockAccount(string sessionId, ulong accountId)
        {
            // 어카운트에 연결된 세션 아이디가 동일한지 확인
            var accountCache = LoadAccountCache(sessionId, accountId);
            if (accountCache.SessionId != sessionId) { return false; }

            return true;
        }

        /// <summary>
        /// 어카운트 캐시 저장
        /// </summary>
        public bool SaveAccountCache(ulong accountId, AccountCache accountCache)
        {
            if (!TryGetAccountCacheKey(accountId, out string key)) { return false; }

            var bytes = _serializer.Dump(accountCache);
            _cache.Set(key, bytes); // TODO: 만료 시간 설정
            return true;
        }


        public AccountCache LoadAccountCache()
        {
            var sessionId = this.Request.SessionId;
            var accountId = this.Session.AccountId;
            return LoadAccountCache(sessionId, accountId);
        }

        /// <summary>
        /// 어카운트 캐시 불러오기
        /// </summary>
        public AccountCache LoadAccountCache(string sessionId, ulong accountId)
        {
            if (!TryGetAccountCacheKey(accountId, out string key)) { return _accountCache; }

            var bytes = _cache.Get(key);
            if (bytes != null)
            {
                var accountCache = _serializer.Load<AccountCache>(bytes);
                if (accountCache != null)
                {
                    // TODO: 만료 시간 갱신
                    _accountCache = accountCache;
                    return _accountCache;
                }
            }

            return _accountCache;
        }
        
        /// <summary>
        /// 요청 파싱 (요청시 한번만 파싱하고 캐시 리턴)
        /// </summary>
        /// <returns>요청 컨텍스트</returns>
        public RequestCache LoadRequestCache()
        {  
            var httpCtx = _accessor.HttpContext;
            if (httpCtx == null) { return _requestCache; }

            if (httpCtx.Request.Query.TryGetValue("SessionId", out var reqSessionId))
            {
                _requestCache.SessionId = reqSessionId;
            }
            else
            {
                // TODO: 헤더 
            }

            if (httpCtx.Request.Query.TryGetValue("Seq", out var reqSeq))
            {
                _requestCache.Seq = ulong.Parse(reqSeq); // TODO: SafeParse
            }
            else
            {
                // TODO: 헤더 
            }

            return _requestCache;
        }

        /// <summary>
        /// 페이로드 업데이트
        /// </summary>
        /// <returns>페이로드</returns>
        public Payload UpdatePayload()
        {
            var request = this.Request;
            var session = this.Session;
            var config = this.Config;
            var logging = this.Logging;

            _payload["Seq"] = request.Seq;
            _payload["SessionId"] = request.SessionId;
            
            _payload["PlayerId"] = session.PlayerId;
            _payload["AccountId"] = session.AccountId;

            _payload["Idfv"] = session.Idfv;
            _payload["Idfa"] = session.Idfa;

            _payload["Project"] = config.Project;
            _payload["Region"] = config.Region;
            _payload["App"] = config.App;
            _payload["Rev"] = config.Rev;

            _payload["Main"] = logging.Main;
            _payload["Sub"] = logging.Sub;

            return _payload;
        }

        /// <summary>
        /// 세션 캐시 키 생성
        /// </summary>
        /// <param name="sessionId">세션 아이디</param>
        /// <param name="outKey">캐시 키 결과</param>
        /// <returns>세션 아이디 유효 여부</returns>
        private bool TryGetSessionCacheKey(string sessionId, out string outKey)
        {
            if (string.IsNullOrEmpty(sessionId)) { outKey = string.Empty; return false; }
            outKey = $"{_config.Project}.Session:{sessionId}";
            return true;
        }

        /// <summary>
        /// 어카운트 캐시 키 생성
        /// </summary>
        /// <param name="accountId">어카운트 아이디</param>
        /// <param name="outKey">캐시 키 결과</param>
        /// <returns>세션 아이디 유효 여부</returns>
        private bool TryGetAccountCacheKey(ulong accountId, out string outKey)
        {
            if (accountId == 0) { outKey = string.Empty; return false; }
            outKey = $"{_config.Project}.Account:{accountId}";
            return true;
        }

        /// <summary>
        /// 캐시 생성 델리게이트
        /// </summary>
        /// <typeparam name="T">캐시 종류</typeparam>
        /// <returns>캐시</returns>
        private delegate T CreateCache<T>();

        /// <summary>
        ///  캐시 
        /// </summary>
        /// <typeparam name="T">캐시 종류</typeparam>
        /// <param name="isCached">캐시 여부</param>
        /// <param name="cache">캐시 참조</param>
        /// <param name="create">캐시 생성 델리게이트</param>
        /// <returns>캐쉬 결과</returns>
        private T Cache<T>(ref bool isCached, ref T cache, CreateCache<T> create)
        {
            if (!isCached)
            {
                cache = create();
                isCached = true;
            }
            return cache;
        }

        private readonly IHttpContextAccessor _accessor;
        private readonly IDistributedCache _cache;
        private readonly ICacheSerializer _serializer;
        private readonly ContextConfig _config;

        private bool _isRequestCached = false;
        private bool _isSessionCached = false;
        private bool _isAccountCached = false;
        private bool _isPayloadCached = false;

        private LoggingCache _loggingCache = new LoggingCache();
        private RequestCache _requestCache = new RequestCache();
        private SessionCache _sessionCache = new SessionCache();
        private AccountCache _accountCache = new AccountCache();
        
        private Dictionary<string, object> _payload = new(); 
    }
}
