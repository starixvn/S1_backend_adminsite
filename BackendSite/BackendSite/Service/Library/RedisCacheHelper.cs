using System;
using System.Threading.Tasks;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BackendSite.Service.Library
{
    public class RedisCacheHelper : ICacheHelper
    {
        private readonly IConnectionMultiplexer _multiplexer;
        
        public RedisCacheHelper(IConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
        }

        public TItem TryGetOrCreate<TItem>(string key, TimeSpan timeSpan, Func<IDatabase, TItem> factory, RedisDataBase redisDataBase = RedisDataBase.AdminSite)
        {
            key = $"{redisDataBase}:{key}";
            var db = _multiplexer.GetDatabase((int)redisDataBase);
            if (!db.KeyExists(key))
            {
                var result = factory(db);

                if (result != null)
                {
                    db.StringSet(key, JsonConvert.SerializeObject(result), timeSpan);
                }

                return result;
            }

            var json = db.StringGet(key);
            return JsonConvert.DeserializeObject<TItem>(json);
        }
        
        public async Task<TItem> TryGetOrCreateAsync<TItem>(string key, TimeSpan timeSpan, Func<IDatabase, Task<TItem>> factory, RedisDataBase redisDataBase = RedisDataBase.AdminSite)
        {
            key = $"{redisDataBase}:{key}";
            var db = _multiplexer.GetDatabase((int)redisDataBase);
            if (!db.KeyExists(key))
            {
                var result = await factory(db);

                if (result != null)
                {
                    await db.StringSetAsync(key, JsonConvert.SerializeObject(result), timeSpan);
                }

                return result;
            }

            var json = db.StringGet(key);
            return JsonConvert.DeserializeObject<TItem>(json);
        }

        public async Task<bool> StringSetAsync(string key, string value, TimeSpan? timeSpan = null, RedisDataBase redisDataBase = RedisDataBase.AdminSite)
        {
            key = $"{redisDataBase}:{key}";
            var db = _multiplexer.GetDatabase((int)redisDataBase);
            return await db.StringSetAsync(key, value, timeSpan);
        }
    }
}