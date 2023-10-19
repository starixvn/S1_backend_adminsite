using BackendSite.Service.BLL.Infrastructure.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL.Infrastructure
{
    public class ConfigBLL
    {
        private readonly Dictionary<string, string> _defaultSiteConfig;
        private readonly Dictionary<int, SiteConfig> _siteIdConfigs;
        private readonly IWebHostEnvironment _environment;

        public ConfigBLL(IServiceProvider provider)
        {
            var siteConfigList = provider.GetServices<SiteConfig>();
            _defaultSiteConfig = siteConfigList.First(i => i.SiteId == 0).Config;
            var siteConfigs = siteConfigList.Where(i => i.SiteId != 0);
            _siteIdConfigs = siteConfigs.ToDictionary(i => i.SiteId, i => i);
            _environment = provider.GetRequiredService<IWebHostEnvironment>();
        }

        public string GetConfigValue(string configKey, int siteId = 0)
        {
            var defaultResult = _defaultSiteConfig.TryGetValue(configKey, out var result) ? result : string.Empty;

            if (siteId == 0)
            {
                return defaultResult;
            }

            if (_siteIdConfigs.TryGetValue(siteId, out var siteConfig))
            {
                return siteConfig.Config.TryGetValue(configKey, out var configValue) ? configValue : defaultResult;
            }

            return defaultResult;
        }

        public TItem GetConfigValue<TItem>(string configKey, int siteId = 0)
        {
            var defaultItem = _defaultSiteConfig.TryGetValue(configKey, out var json)
                ? JsonConvert.DeserializeObject<TItem>(json) : default;

            if (siteId == 0)
            {
                return defaultItem;
            }

            if (_siteIdConfigs.TryGetValue(siteId, out var siteConfig))
            {
                return siteConfig.Config.TryGetValue(configKey, out var configValue)
                   ? JsonConvert.DeserializeObject<TItem>(configValue) : defaultItem;
            }

            return defaultItem;
        }

        public TItem GetConfigValueByEnvironment<TItem>(string configKey, int siteId = 0)
        {
            var json = GetConfigValue($"{configKey}.{_environment.EnvironmentName}.json", siteId);
            return !string.IsNullOrEmpty(json) ? JsonConvert.DeserializeObject<TItem>(json) : default;
        }
    }
}
