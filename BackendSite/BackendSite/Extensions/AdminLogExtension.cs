using BackendSite.Controllers.Helpers;
using Newtonsoft.Json;

namespace BackendSite.Extensions
{
    public static class AdminLogExtension
    {
        public static string ToLog<T>(this T obj)
            => JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings { ContractResolver = new AdminLogResolver() });
    }
}