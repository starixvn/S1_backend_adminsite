using BackendSite.Service.BLL.Infrastructure.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL.Infrastructure
{
    public class ConfigBLL
    {
        private readonly Dictionary<string, string> _defaultSiteConfig;
        private readonly Dictionary<int, SiteConfig> _siteIdConfigs;
        private readonly Dictionary<string, SiteConfig> _siteNameConfigs;
        private readonly IEnumerable<Config> _rawConfigs;
        private readonly IWebHostEnvironment _environment;

        public ConfigBLL(IServiceProvider provider)
        {
            var siteConfigList = provider.GetServices<SiteConfig>();
            _defaultSiteConfig = siteConfigList.First(i => i.SiteId == 0).Config;
            var siteConfigs = siteConfigList.Where(i => i.SiteId != 0);
            _rawConfigs = provider.GetRequiredService<IEnumerable<Config>>();
            _siteNameConfigs = new Dictionary<string, SiteConfig>(siteConfigs.ToDictionary(i => i.SiteName, i => i), StringComparer.OrdinalIgnoreCase);
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

        public IEnumerable<TItem> GetRawConfigValues<TItem>(string configKey)
        {
            var result = new List<TItem>();
            foreach (var config in _rawConfigs.Where(i => i.ConfigKey == configKey))
            {
                var item = JsonConvert.DeserializeObject<TItem>(config.ConfigValue);
                result.Add(item);
            }
            return result;
        }

        public Dictionary<int, TItem> GetConfigValuesBySiteId<TItem>(string configKey)
        {
            TItem defaultItem;
            if (typeof(TItem) == typeof(string))
            {
                defaultItem = (TItem)(object)(_defaultSiteConfig.TryGetValue(configKey, out var json) ? json : null);
            }
            else
            {
                defaultItem = _defaultSiteConfig.TryGetValue(configKey, out var json)
                    ? JsonConvert.DeserializeObject<TItem>(json)
                    : default;
            }
            var result = new Dictionary<int, TItem>();
            foreach (var siteConfig in _siteIdConfigs)
            {
                TItem data;
                if (typeof(TItem) == typeof(string))
                {
                    data = (TItem)(object)(siteConfig.Value.Config.TryGetValue(configKey, out var configValue) ? configValue : (string)(object)defaultItem);
                }
                else
                {
                    data = siteConfig.Value.Config.TryGetValue(configKey, out var configValue)
                        ? JsonConvert.DeserializeObject<TItem>(configValue)
                        : defaultItem;
                }
                result.Add(siteConfig.Key, data);
            }
            return result;
        }

        public Dictionary<string, TItem> GetConfigValuesBySiteName<TItem>(string configKey)
        {
            var defaultItem = _defaultSiteConfig.TryGetValue(configKey, out var json)
                ? JsonConvert.DeserializeObject<TItem>(json) : default;

            var result = new Dictionary<string, TItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var siteConfig in _siteNameConfigs)
            {
                var data = siteConfig.Value.Config.TryGetValue(configKey, out var configValue)
                    ? JsonConvert.DeserializeObject<TItem>(configValue) : defaultItem;
                result.Add(siteConfig.Key, data);
            }
            return result;
        }
    }
}
