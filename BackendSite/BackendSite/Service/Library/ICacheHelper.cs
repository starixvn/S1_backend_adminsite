using BackendSite.Service.Model.enums;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendSite.Service.Library
{
    public interface ICacheHelper
    {
        TItem TryGetOrCreate<TItem>(string key, TimeSpan timeSpan, Func<TItem> factory, RedisDataBase redisDataBase = RedisDataBase.Admin);
        Task<TItem> TryGetOrCreateAsync<TItem>(string key, TimeSpan timeSpan, Func<Task<TItem>> factory, RedisDataBase redisDataBase = RedisDataBase.Admin);
        Task<bool> StringSetAsync(string key, RedisValue value, TimeSpan? timeSpan = null, RedisDataBase redisDataBase = RedisDataBase.Admin);
        bool KeyExists(string key, RedisDataBase redisDataBase = RedisDataBase.Admin);
        Task<TItem> StringGetAsync<TItem>(string key, RedisDataBase redisDataBase = RedisDataBase.Admin);
        List<TItem> GetCacheList<TItem>(IEnumerable<string> keys, RedisDataBase redisDataBase = RedisDataBase.Admin);
        bool SetCacheList<TItem>(Dictionary<string, TItem> dataList, TimeSpan timeSpan, RedisDataBase redisDataBase = RedisDataBase.Admin);
    }
}