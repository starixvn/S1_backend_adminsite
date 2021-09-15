using Newtonsoft.Json;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class FireBaseMessageInfo
    {
        
        public int FirebaseId { get; set; }
        public int SiteId { get; set; }
        public int SendType { get; set; }
        public string CustList { get; set; }
        public string FirebaseTitle { get; set; }
        public string FirebaseBody { get; set; }
        public int SendStatus { get; set; }

        public IEnumerable<int> ICustList
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<int>>(CustList);
            }
        }
        public Dictionary<string, string> IFirebaseTitle
        {
            get
            {
                return  JsonConvert.DeserializeObject<Dictionary<string, string>>(FirebaseTitle) ;
            }
        }
        public Dictionary<string, string> IFirebaseBody
        {
            get
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(FirebaseTitle);
            }
        }

    }
    public class FireBaseMessage
    {
        public int FireBaseId { get; set; }
        public int CustId { get; set; }
        public string SiteName { get; set; }
        public string Token { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int Status { get; set; }
    }
    public class FireBaseReturn
    {
        public int FireBaseId { get; set; }
        public int Status { get; set; }
        public int Success { get; set; }
        public int Fail { get; set; }
    }
    public class FireBaseCustInfo
    {
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string FirebaseToken { get; set; }
        public string UserLang { get; set; }
    }
}
