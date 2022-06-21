using System.Collections.Generic;
using BackendSite.Controllers.Helpers;
using Newtonsoft.Json;

namespace BackendSite.Extensions
{
    public static class AdminLogExtension
    {
        public static string ToLog<T>(this T obj)
            => JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings { ContractResolver = new AdminLogResolver() });
        public static string ToLog<T>(this IEnumerable<T> list)
            => JsonConvert.SerializeObject(list,
                new JsonSerializerSettings { ContractResolver = new AdminLogResolver() });
    }
}