using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class ServerInfoModel
    {
        public int SiteId { set; get; }
        
        public string SiteName { set; get; }
        
        public string LangList { set; get; }
        
        public string CurrencyList { set; get; }
        
        public string DefLang { set; get; }

        public string DefCurrency { set; get; }
        
        public IEnumerable<int> ICurrencyList
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<int>>(CurrencyList);
            }
        }
        public IEnumerable<string> ILangList
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<string>>(LangList);
            }
        }

    }
}
