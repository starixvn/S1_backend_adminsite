using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class Block
    {
    }
    public class BlockIP
    {
        public String IP { get; set; }
        public String Memo { get; set; }
        public String Operator { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Location { get; set; }
    }
    public class BlockBankAcc
    {
        public string UserName { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNum { get; set; }
        public int SysId { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset? _updateTime { get; set; }
        public DateTimeOffset? UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public string UpdateOperator { get; set; }
        public int Status { get; set; }
    }
    public class BlockList 
    {
        public string UserName { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public string AccountName { get; set; }
        public string IFSCCode { get; set; }
    }
    public class IPCountry
    {
        public int id { get; set; }
        public int IPFrom { get; set; }
        public int IPTo { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
    }
    public class IPUser
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
    }
}
