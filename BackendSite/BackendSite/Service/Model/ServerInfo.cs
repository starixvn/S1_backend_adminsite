using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class SiteLanguage
    {
        public string LangId { set; get; }
        public string Language { set; get; }
    }

    public class ServerInfo
    {
        public int SiteId { set; get; }
        public string SiteName { set; get; }
        public string LangList { set; get; }
        public string CurrencyList { set; get; }
        public string DefLang { set; get; }
        public IEnumerable<int> ICurrencyList
        {
            get
            {
                return string.IsNullOrWhiteSpace(CurrencyList) ? 
                    null:
                    JsonConvert.DeserializeObject<IEnumerable<int>>(CurrencyList);
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
    public class AdminOperateLog
    {

        public int? SiteId { set; get; }
        public int? CustId { set; get; }
        public string Menu { set; get; }
        public ActionType ActionType { set; get; }
        public string DetailInfo { set; get; }

    }
    public class AddAdminOperateLog : AdminOperateLog
    {
        public string DetailInfo { set; get; }
        public string Operator { set; get; }
    }
    public class GetAdminOperateLog : AdminOperateLog
    {
        public DateTime CreateTime { set; get; }
        public string Operator { set; get; }
    }
    public class AdminOperateLogReq
    {
        public int CustId { get; set; }
        public string Menu { get; set; }
        public string Operator { get; set; }
        public long startDate { get; set; }
        public long endDate { get; set; }
        public int PageNumber { get; set; }
    }
    public class SiteConfig
    {
        public string BannerURL { get; set; }
    }
}