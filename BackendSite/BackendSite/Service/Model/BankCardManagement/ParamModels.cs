using System.Collections.Generic;
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
            PageSize = req.PageSize;
            StartIndex = req.PageSize.GetStartIndex(req.PageNumber);
        }
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