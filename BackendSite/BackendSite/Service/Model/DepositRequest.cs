using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class DepositRequest
    {
        public string TransId { set; get; }
        public int DispositType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public int SiteId { set; get; }
        public string DispositInfo { set; get; }
        public decimal Amount { set; get; }
        public decimal ActualDeposit { set; get; }
        public DateTime CreateTime { set; get; }
        public IDispositInfo dispositInfo {
            get
            {
                if (DispositType == 3)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<DispositInfo3_1>(DispositInfo);
                }
                return null;
            }
        }
        public int FirstDeposit { set; get; }
        public string Memo { set; get; }
        public string PostScript { set; get; }
        public int Status { set; get; }

        public virtual IEnumerable<Comments> Comments { get; set; }

        public virtual string DispositTypeString { get; set; }
        public virtual string StatusString { get; set; }
    }
    public interface IDispositInfo
    {
        int Ver { get; set; }
    }
    public class DispositInfo1_1 : IDispositInfo
    {
        public int Ver { get; set; }
    }
    public class DispositInfo2_1 : IDispositInfo
    {
        public int Ver { get; set; }
    }
    public class DispositInfo3_1 : IDispositInfo
    {
        public int Ver { get; set; }
        public int SysId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
    }
    public class Comments
    {
        public string Comment { get; set; }
        public DateTime CreateTime { get; set; }
        public int Action { get; set; }

        public string ActionString { get; set; }

        public decimal ActualDeposit { get; set; }
    }
}
