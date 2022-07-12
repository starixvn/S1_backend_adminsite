using System;
using System.Collections.Generic;
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

        public async Task<bool> StringSetAsync(string key, RedisValue value, TimeSpan? timeSpan = null, RedisDataBase redisDataBase = RedisDataBase.Admin)
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

        public bool KeyExists(string key, RedisDataBase redisDataBase = RedisDataBase.Admin)
        {
            key = $"{redisDataBase}:{RemoveSpecialChars(key)}";
            try
            {
                var db = _multiplexer.GetDatabase((int)redisDataBase);
                return db.KeyExists(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{key} | {ex.Message}");
                return false;
            }
        }

        public async Task<TItem> StringGetAsync<TItem>(string key, RedisDataBase redisDataBase = RedisDataBase.Admin)
        {
            key = $"{redisDataBase}:{RemoveSpecialChars(key)}";
            try
            {
                var db = _multiplexer.GetDatabase((int)redisDataBase);
                var json = await db.StringGetAsync(key);
                return JsonConvert.DeserializeObject<TItem>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{key} | {ex.Message}");
                return default;
            }
        }

        public List<TItem> GetCacheList<TItem>(IEnumerable<string> keys, RedisDataBase redisDataBase = RedisDataBase.Admin)
        {            
            try
            {
                var taskList = new List<Task<RedisValue>>();
                var db = _multiplexer.GetDatabase((int)redisDataBase);
                var trans = db.CreateTransaction();
                foreach (var key in keys)
                {
                    var rediskey = $"{redisDataBase}:{RemoveSpecialChars(key)}";
                    var task = db.StringGetAsync(rediskey);
                    taskList.Add(task);
                }

                trans.Execute();

                var reslstList = new List<TItem>();
                foreach (var task in taskList)
                {
                    if (task.Result.HasValue)
                    {
                        var data = JsonConvert.DeserializeObject<TItem>(task.Result);
                        reslstList.Add(data);
                    }
                }
                return reslstList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{keys} | {ex.Message}");
                return default;
            }
        }

        public bool SetCacheList<TItem>(Dictionary<string, TItem> dataList, TimeSpan timeSpan, RedisDataBase redisDataBase = RedisDataBase.Admin)
        {
            try
            {
                var taskList = new List<Task<bool>>();
                var db = _multiplexer.GetDatabase((int)redisDataBase);
                var trans = db.CreateTransaction();
                foreach (var data in dataList)
                {
                    var key = $"{redisDataBase}:{RemoveSpecialChars(data.Key)}";                
                    var task = db.StringSetAsync(key, JsonConvert.SerializeObject(data.Value), timeSpan);
                    taskList.Add(task);
                }
                return trans.Execute();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{dataList.Keys} | {ex.Message}");
                return default;
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