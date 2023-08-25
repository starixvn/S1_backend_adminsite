using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

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

        private readonly Dictionary<int, string> _currencyDescription;
        public string Operator { get; set; }
        public ReferralManagementDecorator(IReferralManagementBLL referralManagementBll, AdjustmentLogReportService adjustmentLogReportService,
            ReportService reportService, ILogger<ReferralManagementDecorator> logger)
        {
            _referralManagementBll = referralManagementBll;
            _adjustmentLogReportService = adjustmentLogReportService;
            _logger = logger;
            _currencyDescription = reportService.GetCurrencyList()
                .ToDictionary(c => c.CurrencyId, c => c.IsoCode);

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
        public async Task<int> SetReferralAsync(ReferralTypeEnum type, SetReferralRequest setReferral)
        {
            var referral = await _referralManagementBll.GetReferralDetail(setReferral.SiteId, setReferral.CurrencyId);

            TransferAfterReferral(referral, type, setReferral);
            var result = await _referralManagementBll.SetReferralAsync(type, setReferral);

            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = Operator,
                SiteId = setReferral.SiteId,
                CurrencyId = setReferral.CurrencyId,
                BeforeDetailInfo = referral == null ? null : JsonConvert.SerializeObject(ToDictionary(referral)),
                AfterDetailInfo = JsonConvert.SerializeObject(ToDictionary(new SetNewReferralRequest(setReferral))),
                ActionType = ActionType.Update,
                MenuEnum = type == ReferralTypeEnum.Referral ? AdminAdjustmentLogMenuEnum.Referral : AdminAdjustmentLogMenuEnum.DepositReferral
            };
            if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
            {
                _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
            }
            return result;
        }
        private void TransferAfterReferral(ReferralDetail referral, ReferralTypeEnum type, SetReferralRequest setReferral)
        {
            if (type == ReferralTypeEnum.Referral)
            {
                setReferral.DepositRewardPercentage = referral.DepositRewardPercentage;
                setReferral.MinPayout = referral.MinPayout;
                setReferral.MaxPayout = referral.MaxPayout;
            }
            else
            {
                setReferral.BonusAmount = referral.BonusAmount;
                setReferral.InvitationLimit = referral.InvitationLimit;
                setReferral.AgentValidBet = referral.AgentValidBet;
                setReferral.AgentDeposit = referral.AgentDeposit;
                setReferral.ReferralFreeCredit = referral.ReferralFreeCredit;
                setReferral.ValidBet = referral.ValidBet;
                setReferral.Deposit = referral.Deposit;
                setReferral.PeriodDays = referral?.PeriodDays;
            }
            var joinSpecificList = referral?.SpecificList?.Where(x => x.Type != type);
            if (joinSpecificList != null)
                setReferral.SpecificList = setReferral.SpecificList.Select(x => new ReferralContent(type, x)).Union(joinSpecificList);
            else
                setReferral.SpecificList = setReferral.SpecificList.Select(x => new ReferralContent(type, x));
        }

        public int SetReferralStatus(int siteId, int currencyId, int status)
        {
            return _referralManagementBll.SetReferralStatus(siteId, currencyId, status);
        }

        public async Task<int> SetReferralStatusAsync(int siteId, int currencyId, ReferralStatusEnum status)
        {
            var referral = await _referralManagementBll.GetReferralDetail(siteId, currencyId);
            if (referral == null)
            {
                throw new AdminException(ErrorCodeEnum.Fail);
            }
            var result = await _referralManagementBll.SetReferralStatusAsync(siteId, currencyId, status);

            var logParam = new CreateAdminAdjustmentInfoLogInput
            {
                Operator = Operator,
                SiteId = siteId,
                CurrencyId = currencyId,
                BeforeDetailInfo = JsonConvert.SerializeObject(new
                {
                    Status = referral.Status.ToString()
                }),
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    Stauts = status.ToString()
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
            return _referralManagementBll.GetReferralCustList(selectSite, userName, currencyId, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }

        public async Task<ReferralCustListPageData> GetReferralCustList(int siteId, string userName, int currencyId, DateTime startDate, DateTime endDate, int pageSize, int pageNumber, bool isOrder, string orderByProperty, OrderByDirection orderByDirection)
        {
            return await _referralManagementBll.GetReferralCustList(siteId, userName, currencyId, startDate, endDate, pageSize, pageNumber, isOrder, orderByProperty, orderByDirection);
        }

        public IEnumerable<ReferralExcelData> GetReferralCustListExport(string selectSite, string userName, int currencyId, DateTime? startDate, DateTime? endDate)
        {
            return _referralManagementBll.GetReferralCustListExport(selectSite, userName, currencyId, startDate, endDate);
        }

        public IEnumerable<ReferralFriend> GetRefFriendList(int custId, DateTime? StartDate, DateTime? EndDate, int PageSize, int PageNumber, out int TotalPages, out int RowCount, bool byRewardTime)
        {
            return _referralManagementBll.GetRefFriendList(custId, StartDate, EndDate, Math.Abs(PageSize), (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize), out TotalPages, out RowCount, byRewardTime);
        }

        public IEnumerable<ReferralFriendRresponse> GetRefFriendList(int custId, DateTime? StartDate, DateTime? EndDate, int PageSize, int PageNumber, out int TotalPages, out int RowCount)
        {
            return _referralManagementBll.GetRefFriendList(custId, StartDate, EndDate, PageSize, PageNumber, out TotalPages, out RowCount);
        }

        private Dictionary<string, object> ToDictionary(object source)
        {
            var type = source.GetType();
            var properties = type.GetProperties();
            var target = new Dictionary<string, object>();
            foreach (var prop in properties)
            {
                if (prop.Name == "CurrencyId")
                {
                    target.Add(prop.Name, _currencyDescription[(int)prop.GetValue(source)!]);
                }
                else if (string.Equals(prop.Name, "SiteId")) { }
                else if (string.Equals(prop.Name, "Status")) { }
                else if (string.Equals(prop.Name, "SpecificList")) { }
                else
                {
                    target.Add(prop.Name, prop.GetValue(source));
                }
            }

            return target;
        }
    }
}