using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class ReferralManagementDecorator : IReferralManagementBLL
    {
        private readonly IReferralManagementBLL _referralManagementBll;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly ILogger<ReferralManagementDecorator> _logger;

        private readonly Dictionary<int, string> _statusDescription = new Dictionary<int, string>()
        {
            { 0, "Disable" },
            { 1, "Enable" }
        };
        public string Operator { get; set; }
        public ReferralManagementDecorator(IReferralManagementBLL referralManagementBll, AdjustmentLogReportService adjustmentLogReportService, ILogger<ReferralManagementDecorator> logger)
        {
            _referralManagementBll = referralManagementBll;
            _adjustmentLogReportService = adjustmentLogReportService;
            _logger = logger;
        }
        public IEnumerable<ReferralList> GetReferralBySiteId(int siteId)
        {
            return _referralManagementBll.GetReferralBySiteId(siteId);
        }

        public async Task<ReferralDetail> GetReferralDetail(int siteId, int currencyId)
        {
            return await _referralManagementBll.GetReferralDetail(siteId, currencyId);
        }

        public void SetReferral(List<ReferralManagement> referralList)
        {
            _referralManagementBll.SetReferral(referralList);
        }
        public async Task<int> SetReferralAsync(SetReferralRequest setReferral)
        {
            var referral = await _referralManagementBll.GetReferralDetail(setReferral.SiteId, setReferral.CurrencyId);
            
            var result = await _referralManagementBll.SetReferralAsync(setReferral);
            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = Operator,
                SiteId = setReferral.SiteId,
                CurrencyId = setReferral.CurrencyId,
                BeforeDetailInfo = referral?.ToLog(),
                AfterDetailInfo = setReferral.ToLog(),
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.Referral
            };
            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
            }
            return result;
        }
        public int SetReferralStatus(int siteId, int currencyId,int status)
        {
            var referralList = _referralManagementBll.GetReferralBySiteId(siteId).ToList();
            var referral = referralList.FirstOrDefault(x => x.CurrencyId == currencyId);

            if (referral == null)
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }
            var result =_referralManagementBll.SetReferralStatus(siteId, currencyId, status);
            
            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = Operator,
                SiteId = siteId,
                CurrencyId = currencyId,
                BeforeDetailInfo = JsonConvert.SerializeObject(new
                {
                    Status = referral.Status ? _statusDescription[1] : _statusDescription[0]
                }),
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    Stauts = _statusDescription[status]
                }),
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.Referral
            };
            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
            }

            return result;
            
        }

        public IEnumerable<ReferralCustList> GetReferralCustList(IEnumerable<int> selectSite, string userName, int currencyId, DateTime? startDate, DateTime? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return _referralManagementBll.GetReferralCustList(selectSite, userName, currencyId, startDate, endDate , pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public IEnumerable<ReferralExcelData> GetReferralCustListExport(string selectSite, string userName, int currencyId , DateTime? startDate, DateTime? endDate)
        {
            return _referralManagementBll.GetReferralCustListExport(selectSite, userName, currencyId, startDate, endDate);
        }

        public IEnumerable<ReferralFriend> GetRefFriendList(int custId, DateTime? StartDate, DateTime? EndDate, int PageSize, int PageNumber, out int TotalPages, out int RowCount, bool byRewardTime)
        {
            return _referralManagementBll.GetRefFriendList(custId, StartDate, EndDate, Math.Abs(PageSize), (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize), out TotalPages, out RowCount, byRewardTime);
        }
    }
}