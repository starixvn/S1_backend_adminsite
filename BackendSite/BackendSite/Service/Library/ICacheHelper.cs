using BackendSite.Service.Model.enums;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace BackendSite.Service.Library
{
    public interface ICacheHelper
    {
        TItem TryGetOrCreate<TItem>(string key, TimeSpan timeSpan, Func<TItem> factory, RedisDataBase redisDataBase = RedisDataBase.AdminSite);
        Task<TItem> TryGetOrCreateAsync<TItem>(string key, TimeSpan timeSpan, Func<Task<TItem>> factory, RedisDataBase redisDataBase = RedisDataBase.AdminSite);
        Task<bool> StringSetAsync(string key, string value, TimeSpan? timeSpan = null, RedisDataBase redisDataBase = RedisDataBase.AdminSite);
    }
}