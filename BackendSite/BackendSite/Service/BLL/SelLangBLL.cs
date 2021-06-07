using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class SelLangBLL : ISelLang
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private ConcurrentDictionary<string, ResourceManager> rm = new ConcurrentDictionary<string, ResourceManager>();
        private string sLangId;

        public SelLangBLL()
        {

        }

        public SelLangBLL(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.sLangId = null;
        }

        public IEnumerable<SiteLanguage> GetLangIdList(ServerInfo serverInfo)
        {
            List<SiteLanguage> list = new List<SiteLanguage>();

            foreach (var langId in serverInfo.ILangList)
            {
                SiteLanguage siteLanguage = new SiteLanguage();
                siteLanguage.LangId = langId;
                siteLanguage.Language = GetLan_Info(langId);
                list.Add(siteLanguage);
            }

            return list;
        }
        private string GetLan_Info(string key)
        {
            //
            string labelString = string.Empty;
            string sKey = $"{Assembly.GetExecutingAssembly().GetName().Name}.App_GlobalResources.Lan_Info";
            if (!rm.ContainsKey(sKey))
            {
                rm.TryAdd(sKey, new ResourceManager(sKey, Assembly.GetExecutingAssembly()));
            }
            labelString = rm[sKey].GetString(key);

            return labelString;
        }

        private string GetResourceValue(string key, string categoryId, string langId)
        {
            //
            string labelString = string.Empty;
            string sKey = $"{Assembly.GetExecutingAssembly().GetName().Name}.App_GlobalResources.{categoryId}_{langId}";
            if (!rm.ContainsKey(sKey))
            {
                rm.TryAdd(sKey, new ResourceManager(sKey, Assembly.GetExecutingAssembly()));
            }

            try
            {
                labelString = rm[sKey].GetString(key);
            }
            catch(MissingManifestResourceException mme)
            {

            }

            if (string.IsNullOrEmpty(labelString) && langId != "en")
            {
                labelString = GetResourceValue(key, categoryId, "en");
            }

            return labelString;
        }
        public string GetResourceValue(string key)
        {
            string labelString = GetResourceValue(key, "lan", "en");

            return labelString;
        }
        public string GetMsg(string key)
        {
            string labelString = GetResourceValue(key, "msg", "en");

            return labelString;
        }
        public string GetResourceValue(string key,string lang)
        {
            string labelString = GetResourceValue(key, "lan", lang);

            return labelString;
        }
        public string GetMsg(string key,string lang)
        {
            string labelString = GetResourceValue(key, "msg", lang);

            return labelString;
        }
        public string GetErrMsg(int code)
        {
            string labelString = GetResourceValue($"Code_{code}", "GameMsg_lan", "en");

            return labelString;
        }
    }
}
