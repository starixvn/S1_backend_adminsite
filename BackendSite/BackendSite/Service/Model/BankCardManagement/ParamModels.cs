using BackendSite.Extensions;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankCardManagement
{
    public class GetOfflineBankingSettingParams : PagerParamModel
    {
        public GetOfflineBankingSettingParams(){}
        public GetOfflineBankingSettingParams(GetOfflineBankingSettingRequest req)
        {
            SiteId = req.SiteId;
            CurrencyId = req.CurrencyId;
            Purposes = req.Purposes;
            Statuses = req.Statuses;
            SysIds = req.SysIds;
            VipLevels = req.VipLevels;
            PointLevels = req.PointLevels;
            AccountNo = req.AccountNo;
            Agent = req.Agent;
            PageSize = req.PageSize;
            StartIndex = req.PageSize.GetStartIndex(req.PageNumber);
        }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<TransferAccountPurposeEnum> Purposes { get; set; }
        public IEnumerable<BankCardManagementStatusEnum> Statuses { get; set; }
        public IEnumerable<int> SysIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> PointLevels { get; set; }
        public string AccountNo { get; set; }
        
        public string Agent { get; set; }
    }
    
    public class SetOfflineBankingSettingParams
    {
        public SetOfflineBankingSettingParams(SetOfflineBankingSettingRequest req, string updater)
        {
            FinancialAccountId = req.FinancialAccountId;
            MinAmount = req.MinAmount;
            MaxAmount = req.MaxAmount;
            DailyLimitAmount = req.DailyLimitAmount;
            PointLevels = req.PointLevels;
            VipLevels = req.VipLevels;
            Memo = req.Memo;
            Sort = req.Sort;
            Status = req.Status;
            Operator = updater;
        }
        public int FinancialAccountId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal DailyLimitAmount { get; set; }
        public IEnumerable<int> PointLevels { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public string Memo { get; set; }
        public int Sort { get; set; }
        public BankCardManagementStatusEnum Status { get; set; }
        public string Operator { get; set; }
    }
}