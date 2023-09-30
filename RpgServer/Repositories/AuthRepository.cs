using Microsoft.Extensions.Caching.Distributed;
using RpgServer.Configs;
using RpgServer.Databases;

namespace RpgServer.Repositories
{
    using Models;
    using Extensions;
    using System.Security.Principal;
    using Microsoft.AspNetCore.Http;
    using RpgServer.Serializers;

    public class AuthRepository
    {
        public AuthRepository(IDistributedCache cache, ICacheSerializer serializer, ContextConfig config, AuthDatabase authDb)
        {
            _cache = cache;
            _serializer = serializer;
            _config = config;
            _authDb = authDb;
        }

        public DeviceModel? LoadDevice(string inIdfv)
        {
            string cacheKey = _cache.GenIdfvKey(inIdfv);
            return LoadModel(cacheKey, inIdfv, _authDb.FindDevice, _config.DeviceCacheOpts);
        }

        public DeviceModel GenDevice(string inIdfv, ulong inAccountId)
        {
            var newDevice = new DeviceModel()
            {
                Idfv = inIdfv,
                AccountId = inAccountId
            };
            _authDb.Add(newDevice);
            _authDb.SaveChanges();

            string cacheKey = _cache.GenIdfvKey(inIdfv);
            Cache(cacheKey, newDevice, _config.DeviceCacheOpts);

            return newDevice;
        }

        public AccountModel? LoadAccount(ulong inAccountId)
        {
            string cacheKey = _cache.GenAccountKey(inAccountId);
            return LoadModel(cacheKey, inAccountId, _authDb.FindAccount, _config.AccountCacheOpts);
        }

        public AccountModel GenAccount(string inIdfv)
        {
            var newSessionId = GenSessionId();
            var newAccount = new AccountModel()
            {
                SessionId = newSessionId,
                Idfv = inIdfv
            };

            _authDb.Add(newAccount);
            _authDb.SaveChanges();

            string cacheKey = _cache.GenAccountKey(newAccount.Id);
            Cache(cacheKey, newAccount, _config.AccountCacheOpts);

            return newAccount;
        }
        public SessionModel TouchSession(AccountModel account, DeviceModel device)
        {
            var boundSession = LoadSession(account.SessionId);
            if (boundSession == null)
            {
                var foundSession = _authDb.FindSessionByAccountId(account.Id);
                if (foundSession == null)
                {
                    return GenSession(account, device);
                }
                else
                {
                    return foundSession;
                }
            }
            else
            {
                return boundSession;
            }
        }

        public SessionModel? LoadSession(string inSessionId)
        {
            string cacheKey = _cache.GenSessionKey(inSessionId);
            return LoadModel(cacheKey, inSessionId, _authDb.FindSessionById, _config.SessionCacheOpts);
        }

        public SessionModel GenSession(AccountModel inAccount, DeviceModel inDevice)
        {
            var newSession = new SessionModel()
            {
                Id = inAccount.SessionId,
                AccountId = inAccount.Id,
                Idfv = inDevice.Idfv
            };

            _authDb.Add(newSession);
            _authDb.SaveChanges();

            string cacheKey = _cache.GenSessionKey(newSession.Id);
            Cache(cacheKey, newSession, _config.SessionCacheOpts);
            return newSession;
        }

        public void ResetSessionId(SessionModel inSession, AccountModel inAccount, DeviceModel inDevice)
        {
            var newSessionId = GenSessionId();
            var oldSessionId = inSession.Id;

            // 세션 캐시 삭제
            string oldSessionCacheKey = _cache.GenSessionKey(oldSessionId);
            _cache.Remove(oldSessionCacheKey);

            // 모델 갱신
            inSession.Id = newSessionId; // 세션 아이디 변경
            inAccount.Idfv = inDevice.Idfv; // 최근 접속한 디바이스 아이디 설정
            inAccount.SessionId = newSessionId; // 중복 로그인 방지를 위해 어카운트 세션 아이디 갱신

            // 데이터 베이스 갱신
            _authDb.Update(inSession);
            _authDb.Update(inAccount);
            _authDb.SaveChanges();

            // 어카운트 캐시 갱신
            string cacheKey = _cache.GenAccountKey(inAccount.Id);
            Cache(cacheKey, inAccount, _config.AccountCacheOpts);

            // 세션 캐시 추가
            string newSessionCacheKey = _cache.GenSessionKey(inSession.Id);
            Cache(newSessionCacheKey, inSession, _config.SessionCacheOpts);
        }

        private string GenSessionId()
        {
            return Guid.NewGuid().ToString();
        }

        private delegate T FindModel<T, I>(I inModelId);

        private T? LoadModel<T, I>(string inCacheKey, I inModelId, FindModel<T, I> findModel, DistributedCacheEntryOptions cacheOpts)
        {
            var cachedBytes = _cache.Get(inCacheKey);
            if (cachedBytes != null)
            {
                var cachedModel = _serializer.Load<T>(cachedBytes);
                if (cachedModel != null)
                {
                    return cachedModel;
                }
            }

            var dbModel = findModel(inModelId);
            if (dbModel == null) return default;

            Cache(inCacheKey, dbModel, cacheOpts);
            return dbModel;
        }

        private void Cache<T>(string inCacheKey, T inModel, DistributedCacheEntryOptions cacheOpts)
        {
            var dbBytes = _serializer.Dump(inModel);
            _cache.Set(inCacheKey, dbBytes, cacheOpts);
        }

        private readonly IDistributedCache _cache;
        private readonly ICacheSerializer _serializer;

        private readonly ContextConfig _config;
        private readonly AuthDatabase _authDb;
    }
}
