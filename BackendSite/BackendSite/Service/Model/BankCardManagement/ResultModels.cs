using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.CloudFront.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.BankCardManagement
{
    public class OfflineBankingSetting
    {
        private DateTimeOffset _lastUpdateTime;
        public int FinancialAccountId { set; get; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        /// <summary>
        /// 目前只有 INR IFSCCode 使用
        /// </summary>
        public string AccountCode { set; get; }
        public decimal MinAmount { set; get; }
        public decimal MaxAmount { set; get; }
        public decimal DailyLimitAmount { set; get; }
        [JsonIgnore]
        public string FinancialAccountPurposes { set; get; }
        private IEnumerable<TransferAccountPurposeEnum> AccountPurposes =>
            JsonConvert.DeserializeObject<IEnumerable<TransferAccountPurposeEnum>>(FinancialAccountPurposes);

        public IEnumerable<string> Purposes
            => AccountPurposes
                .Where(x => x != TransferAccountPurposeEnum.Transfer)
                .Select(x => x.ToString());
        
        public int CurrencyId { set; get; }
        public int SysId { set; get; }
        public string BankCode { set; get; }
        public string BankName { set; get; }
        public BankDetailOffStatusEnum Status { set; get; }
        public IEnumerable<int> PointLevels { set; get; }
        public IEnumerable<int> VipLevels { set; get; }
        public IEnumerable<string> VipLevelNames { set; get; }
        public decimal Dummy { set; get; }
        public string Memo { set; get; }
        public int Sort { set; get; }
        public DateTimeOffset LastUpdateTime
        {
            get => _lastUpdateTime;
            set => _lastUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string LastUpdater { set; get; }
    }
}