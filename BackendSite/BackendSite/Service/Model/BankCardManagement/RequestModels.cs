using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankCardManagement
{
    public class GetOfflineBankingSettingRequest : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<TransferAccountPurposeEnum> Purposes { get; set; }
        public IEnumerable<BankCardManagementStatusEnum> Statuses { get; set; }
        public IEnumerable<int> SysIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> PointLevels { get; set; }
        public IEnumerable<BankCardManagementSupportChannelEnum> SupportChannels { get; set; }
        public string AccountNo { get; set; }
        public int AgentId { get; set; }
    }

    public class SetOfflineBankingSettingRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int FinancialAccountId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal DailyLimitAmount { get; set; }
        public IEnumerable<int> PointLevels { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public string Memo { get; set; }
        public int Sort { get; set; }
        public BankCardManagementStatusEnum Status { get; set; }
        public IEnumerable<BankCardManagementSupportChannelEnum> SupportChannels { get; set; }
        public string QrCodeUrl { get; set; }
        public IEnumerable<int> GroupIds { get; set; }
    }
}