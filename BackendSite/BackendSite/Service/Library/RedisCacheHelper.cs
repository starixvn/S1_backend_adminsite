using System;
using System.Threading.Tasks;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BackendSite.Service.Library
{
    public class RedisCacheHelper : ICacheHelper
    {
        private readonly ILogger<ICacheHelper> _logger;
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly string[] _charsToRemove = { "[", "]", "/", " ", "," };
        
        public RedisCacheHelper(IServiceProvider provider)
        {
            _logger = provider.GetRequiredService<ILogger<ICacheHelper>>();
            _multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
        }

        public TItem TryGetOrCreate<TItem>(string key, TimeSpan timeSpan, Func<TItem> factory, RedisDataBase redisDataBase = RedisDataBase.Admin) 
        {
            key = $"{redisDataBase}:{RemoveSpecialChars(key)}";
            try
            {                      
                var db = _multiplexer.GetDatabase((int)redisDataBase);
                
                if (!db.KeyExists(key))
                {
                    var result = factory();
                    if (result != null)
                    {
                        db.StringSet(key, JsonConvert.SerializeObject(result), timeSpan);
                    }
                    return result;
                }

                var json = db.StringGet(key);
                return JsonConvert.DeserializeObject<TItem>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"{key} | {ex.Message}");
                return factory();
            }
        }
        
        public async Task<TItem> TryGetOrCreateAsync<TItem>(string key, TimeSpan timeSpan, Func<Task<TItem>> factory, RedisDataBase redisDataBase = RedisDataBase.Admin)
        {
            key = $"{redisDataBase}:{RemoveSpecialChars(key)}";
            try
            {                      
                var db = _multiplexer.GetDatabase((int)redisDataBase);
                
                if (!db.KeyExists(key))
                {
                    var result = await factory();
                    if (result != null)
                    {
                        await db.StringSetAsync(key, JsonConvert.SerializeObject(result), timeSpan);
                    }
                    return result;
                }

                var json = db.StringGet(key);
                return JsonConvert.DeserializeObject<TItem>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{key} | {ex.Message}");
                return await factory();
            }
        }

        public async Task<bool> StringSetAsync(string key, string value, TimeSpan? timeSpan = null, RedisDataBase redisDataBase = RedisDataBase.Admin)
        {
            key = $"{redisDataBase}:{RemoveSpecialChars(key)}";
            try
            {           
                var db = _multiplexer.GetDatabase((int)redisDataBase);
                return await db.StringSetAsync(key, value, timeSpan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{key} | {ex.Message}");
                return false;
            }
           
        }
        
        private string RemoveSpecialChars(string key)
        {
            foreach (var c in _charsToRemove)
            {
                key = key.Replace(c, string.Empty);
            }

            return key;
        }
    }
}