using System.Collections.Generic;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankCardManagement
{
    public class GetOfflineBankingSettingRequest : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<TransferAccountPurposeEnum> Purposes { get; set; }
        public IEnumerable<BankDetailOffStatusEnum> Statuses { get; set; }
        public IEnumerable<int> SysIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> PointLevels { get; set; }
        public string AccountNo { get; set; }
    }
    
    public class SetOfflineBankingSettingRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<TransferAccountPurposeEnum> Purposes { get; set; }
        public IEnumerable<BankDetailOffStatusEnum> Statuses { get; set; }
        public IEnumerable<int> SysIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> PointLevels { get; set; }
        public string AccountNo { get; set; }
    }
}