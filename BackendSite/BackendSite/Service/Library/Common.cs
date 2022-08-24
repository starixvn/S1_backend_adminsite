using BackendSite.Service.BLL;
using BackendSite.Service.Model.Option;
using System.Text.RegularExpressions;

namespace BackendSite.Service.Library
{
    public class Common
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IReadOnlyDictionary<int, TpListOption> _tpList;
        private readonly IReadOnlyDictionary<int, CurrencyOption> _currencyList;
        private readonly IReadOnlyDictionary<int, string> _siteNameList;
        private readonly ServerInfoBLL serverInfoBLL;

        public Common(IServiceProvider provider)
        {
            _environment = provider.GetRequiredService<IWebHostEnvironment>();
            _tpList = provider.GetRequiredService<IEnumerable<TpListOption>>().ToDictionary(d => d.Id, d => d);
            _currencyList = provider.GetRequiredService<IEnumerable<CurrencyOption>>().ToDictionary(d => d.CurrencyId, d => d);
            _siteNameList = provider.GetRequiredService<ServerInfoBLL>().GetSiteList().ToDictionary(d => d.SiteId, d => d.SiteName);
        }
        public string CreateSerialId()
        {
            return $"{GetEnv()}{DateTime.Now.ToString("yyMMddhhmmss")}{new Random(Guid.NewGuid().GetHashCode()).Next(10000, 99999)}";
        }
        public string GetEnv()
        {
            if (_environment.EnvironmentName == "Development")
            {
                return "D";
            }
            else if (_environment.EnvironmentName == "Staging")
            {
                return "S";
            }
            else if (_environment.EnvironmentName == "Production")
            {
                return "P";
            }
            else if (_environment.EnvironmentName == "QA")
            {
                return "Q";
            }
            return "X";
        }
        public int? TpIdToCustId(string TpId)
        {
            TpId = TpId.Trim();

            if (TpId.IndexOf("_") > -1)
            {
                TpId = TpId.Substring(TpId.LastIndexOf("_"), TpId.Length - TpId.LastIndexOf("_"));
            }
            else if (TpId.IndexOf("x") > -1)
            {
                TpId = TpId.Substring(TpId.LastIndexOf("x"), TpId.Length - TpId.LastIndexOf("x"));
            }

            var m = Regex.Match(TpId, @"\d+");
            var _custId = int.TryParse(m.Value, out var custId) ? custId : (int?)null;

            return _custId;
        }

        public string TransferToTimeZoneFormat(int timeZone)
        {
            if (Math.Sign(timeZone) > 0 || Math.Sign(timeZone) == 0)
            {
                return string.Format("+" + Math.Abs(timeZone).ToString("D2") + ":00");
            }
            else
            {
                return string.Format("-" + Math.Abs(timeZone).ToString("D2") + ":00");
            }
        }

        public (int StartIndex, int PageSize) CalculatePageStartIndexAndPageSize(int pageNumber, int pageSize)
        {
            int newPageSize = Math.Abs(pageSize);
            int startIndex = (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize);
            return (startIndex, newPageSize);
        }

        public string GetCurrencyIsoCode(int currencyId)
        {
            return _currencyList.GetValueOrDefault(currencyId).IsoCode;
        }

        public string GetTpName(int tpId)
        {
            return _tpList.TryGetValue(tpId, out var result) ? result.TpName : string.Empty;
        }

        public string GetSiteName(int siteId)
        {
            return _siteNameList.GetValueOrDefault(siteId, string.Empty);
        }
    }
}
