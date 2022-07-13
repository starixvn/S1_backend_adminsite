using BackendSite.Attributes;
using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class EWallet
    {
        public string UserName { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public string AccountName { get; set; }
    }
    public class BlockEWalletAcc
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string EWalleName { get; set; }
        public string AccountNum { get; set; }
        public int SysId { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset? _blockUpdateTime { get; set; }
        public DateTimeOffset? BlockUpdateTime
        {
            get => _blockUpdateTime;
            set => _blockUpdateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public string UpdateOperator { get; set; }
        public int Status { get; set; }
        public int BlockStatus { get; set; }
        public int CurrencyId { get; set; }
    }
    public class EWalletInfoSys
    {
        public int SysId { get; set; }
        public string EWalleName { get; set; }
        public int CurrencyId { get; set; }
    }
    public class EWalletUpdateRequest
    {
        public int WithdrawalMin { get; set; }
        public int WithdrawalMax { get; set; }
    }
    public class EWalletSwitchStatusRequest
    {
        [LogIgnore]
        public EWalletStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
    }
}
