using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BackendSite.Service.Library
{
    public class RedisHelper
    {
        private readonly IDatabase _redis;
        public RedisHelper(IServiceProvider provider)
        {
            _redis = provider.GetRequiredService<IDatabase>();
        }

        public TItem TryGetOrCreate<TItem>(string key, TimeSpan timeSpan, Func<IDatabase, TItem> factory)
        {
            if (!_redis.KeyExists(key))
            {
                var result = factory(_redis);

                if (result != null)
                {
                    _redis.StringSet(key, JsonConvert.SerializeObject(result), timeSpan);
                }

                return result;
            }

            var json = _redis.StringGet(key);
            return JsonConvert.DeserializeObject<TItem>(json);
        } 
    }
}