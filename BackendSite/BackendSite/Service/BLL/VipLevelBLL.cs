using BackendSite.Extensions;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class VipLevelBLL : IVipLevelBLL
    {
        private readonly VipLevelService _vipLevelService;
        private readonly IMemberService _memberService;
        private readonly IAdminLogService _adminLogService;
        private readonly ILogger<VipLevelBLL> _logger;

        private readonly List<VipLevelSettingDto> _vipLevelSettingList = new()
        {
            new VipLevelSettingDto { SiteId = 0, Type = UpgradeSettingEnum.UpBonus, TypeName = "Tier Upgrade Bonus" },
            new VipLevelSettingDto { SiteId = 0, Type = UpgradeSettingEnum.UpDeposit, TypeName = "Monthly Minimum Deposit" },
            new VipLevelSettingDto { SiteId = 0, Type = UpgradeSettingEnum.UpTurnover, TypeName = "Monthly Minimum Turnover" },
            new VipLevelSettingDto { SiteId = 0, Type = UpgradeSettingEnum.DownDeposit, TypeName = "Minimum Deposit Within Validity Period" },
            new VipLevelSettingDto { SiteId = 0, Type = UpgradeSettingEnum.DownTurnover, TypeName = "Minimum Turnover Within Validity Period" },
            new VipLevelSettingDto { SiteId = 101, Type = UpgradeSettingEnum.UpDeposit, TypeName = "Deposit Requirement" },
            new VipLevelSettingDto { SiteId = 101, Type = UpgradeSettingEnum.UpTurnover, TypeName = "Turnover Requirement" },
            new VipLevelSettingDto { SiteId = 101, Type = UpgradeSettingEnum.DownDeposit, TypeName = "Retain Deposit" },
            new VipLevelSettingDto { SiteId = 101, Type = UpgradeSettingEnum.DownTurnover, TypeName = "Retain Turnover" }
        };

        public VipLevelBLL(VipLevelService vipLevelService, IMemberService memberService, IAdminLogService adminLogService, ILogger<VipLevelBLL> logger)
        {
            _vipLevelService = vipLevelService;
            _memberService = memberService;
            _adminLogService = adminLogService;
            _logger = logger;
        }

        public IEnumerable<VipLevelSetting> GetVipLevelSetting(int siteId)
        {
            return _vipLevelService.GetVipLevelSetting(siteId);
        }

        public async Task<IEnumerable<VipLevelSetting>> GetVipLevelSettingAsync(int siteId)
        {
            return await _vipLevelService.GetVipLevelSettingAsync(siteId);
        }

        public VipLevelInfo GetVipLevelInfo(int custId)
        {
            return _vipLevelService.GetVipLevelInfo(custId);
        }

        public async Task<MemberVipLevelInfo> GetVipLevelInfoAsync(int custId)
        {
            var data = await _vipLevelService.GetVipLevelInfoAsync(custId);
            if (data == null) return null;

            return new MemberVipLevelInfo()
            {
                Manually = data.Manually,
                DefaultLevel = data.DefaultLevel,
                VipLevel = data.VipLevel,
                QualifiedTime = data.QualifiedTime
            }; ;

        }

        public IEnumerable<VipLevelLog> GetVipLevelLog(int custId, int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return _vipLevelService.GetVipLevelLog(custId,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }

        public async Task<IEnumerable<MemberVipLevelLog>> GetVipLevelLogAsync(int custId)
        {
            var data = await _vipLevelService.GetVipLevelLogAsync(custId);
            var vipLevelLog = data.Select(a => new MemberVipLevelLog()
            {
                OldLevel = a.OldLevel,
                NewLevel = a.NewLevel,
                Memo = a.Memo,
                UpdateTime = new DateTimeOffset(a.UpdateTime, TimeSpan.Zero),
                Updaters = a.Updaters
            });
            return vipLevelLog;
        }
        
        public async Task<int> SetVipLevelAsync(int siteId, int custId, string userName, int vipLevel, string updator)
        {
            return await _vipLevelService.SetVipLevelAsync(siteId, custId, userName, vipLevel, $"{updator} Update VipLevel", updator);
        }
        
        public async Task<int> SetVipLevelManuallyAsync(int siteId, int custId, string userName, int manually, string updator)
        {
            return await _vipLevelService.SetVipLevelManuallyAsync(siteId, custId, userName, manually, $"{updator} Update Manually", updator);
        }

        public async Task<int> SetVipLevelDefaultAsync(int siteId, int custId, string userName, int oldLevel, DefaultVIPLevel defaultLevel, string updator)
        {
            var newLevel = (oldLevel, defaultLevel) switch
            {
                ((int)DefaultVIPLevel.BlackTortoise, DefaultVIPLevel.Qilin) => (int)DefaultVIPLevel.Qilin,
                ((int)DefaultVIPLevel.Qilin, DefaultVIPLevel.BlackTortoise) => (int)DefaultVIPLevel.BlackTortoise,
                _ => oldLevel
            };
            
            return await _vipLevelService.SetVipLevelDefaultAsync(siteId, custId, userName, oldLevel , newLevel, (int)defaultLevel , $"{updator} Update Default {defaultLevel.ToString()}", updator);
        }
        
        public async Task<int> VipAutomationExclusion(int custId, bool enable, string updator)
        {
            var customer = await _memberService.GetCustomerBasicInfoAsync(custId);
            if (customer == null) return -1;

            var beforeStatus = customer.Exclusive;
            var isExclusive = enable ? 1 : 0;
            if (beforeStatus == isExclusive) return -2;

            var result = await _vipLevelService.SetAutoCustVipExclusionAsync(customer.SiteId, custId, customer.UserName, isExclusive);
            if (result <= 0) return -3;

            var afterStatus = isExclusive;
            await CreateAdminLog(updator, customer.SiteId, customer.CurrencyId,
                JsonConvert.SerializeObject(new { customer.UserName, VipAutomationExclusion = beforeStatus.ToString() }),
                JsonConvert.SerializeObject(new { customer.UserName, VipAutomationExclusion = afterStatus.ToString() }), ActionType.Update,
                custId, $"Edit Exclusive \"{afterStatus}\"");
            return result;
        }
        
        #region Public Methods

        public GetVipUpgradeReviewListModel GetVipUpgradeReviewList(int siteId, string calculateYearMonth, string selectStatus, string userName, int pageNumber, int pageSize, string currencyIds)
        {
            var data = GetVipUpgradeReview(siteId, calculateYearMonth, selectStatus, userName, pageNumber, pageSize, currencyIds, out var totalPages, out var rowCount);

            var details = data.Select(p => new VipUpgradeReviewDetailModel(p)).ToList();

            if (CheckCanUpdateStatus(calculateYearMonth, siteId, VipSettingStatusEnum.Upgrade))
            {
                details = details.Select(c =>
                {
                    c.CanUpdateStatus = true;
                    return c;
                }).ToList();
            }

            details = details.Select(c =>
            {
                if (c.Status == VipStatus.Settled)
                {
                    c.QualifiedTime = null;
                }
                return c;
            }).ToList();

            return new GetVipUpgradeReviewListModel()
            {
                Data = details,
                PageNumber = Math.Abs(pageNumber),
                RowCount = rowCount,
                TotalPages = totalPages
            };
        }

        public IEnumerable<VipUpgradeReviewDataModel> ExportVipUpgradeReviewList(int siteId, string calculateYearMonth, string selectStatus, string userName, string currencyIds)
        {
            return GetVipUpgradeReview(siteId, calculateYearMonth, selectStatus, userName, 0, 0, currencyIds, out _, out _);
        }

        public bool UpdateVipUpgradeReview(IEnumerable<int> custIdList, string calculateYearMonth, int siteId, VipStatus newStatus, string updater)
        {
            return CheckCanUpdateStatus(calculateYearMonth, siteId, VipSettingStatusEnum.Upgrade) && UpdateVipUpgradeReview(custIdList, calculateYearMonth, newStatus, updater);
        }

        public GetVipDowngradeReviewListModel GetVipDowngradeReviewList(int siteId,
            string calculateYearMonth,
            string selectStatus,
            string currencyId,
            string userName,
            int pageNumber,
            int pageSize)
        {
            var data = GetVipDowngradeReview(siteId,
                calculateYearMonth,
                selectStatus,
                currencyId,
                userName,
                pageNumber,
                pageSize,
                out var totalPages,
                out var rowCount).ToList();

            var details = data.Select(p => new VipDowngradeReviewDetailModel(p)).ToList();

            if (CheckCanUpdateStatus(calculateYearMonth, siteId, VipSettingStatusEnum.Downgrade))
            {
                details = details.Select(c =>
                {
                    // 排除名單才可以手動調整
                    c.CanUpdateStatus = c.Exclusive;
                    return c;
                }).ToList();
            }

            details = details.Select(c =>
            {
                if (c.Status == VipStatus.Settled)
                {
                    c.QualifiedTime = null;
                }
                return c;
            }).ToList();

            return new GetVipDowngradeReviewListModel()
            {
                Data = details,
                PageNumber = Math.Abs(pageNumber),
                RowCount = rowCount,
                TotalPages = totalPages
            };
        }

        public IEnumerable<VipDowngradeReviewDataModel> ExportVipDowngradeReviewList(int siteId, string calculateYearMonth, string selectStatus, string currencyId, string userName)
        {
            return GetVipDowngradeReview(siteId, calculateYearMonth, selectStatus, currencyId, userName, 0, 0, out _, out _);
        }

        public bool UpdateVipDowngradeReview(IEnumerable<int> custIdList, string calculateYearMonth, int siteId, VipStatus newStatus, string updater)
        {
            return CheckCanUpdateStatus(calculateYearMonth, siteId, VipSettingStatusEnum.Downgrade) && UpdateVipDowngradeReview(custIdList, calculateYearMonth, newStatus, updater);
        }

        public async Task<IEnumerable<VipUpgradeSetting>> GetUpgradeSetting(int siteId, int currencyId)
        {
            var vipSystem = await _vipLevelService.GetVipSystem(siteId);
            var currencyVipSystem = vipSystem.ToList().FindAll(x => x.CurrencyId == currencyId);
            var result = new List<VipUpgradeSetting>();
           
            foreach (UpgradeSettingEnum type in Enum.GetValues(typeof(UpgradeSettingEnum)))
            {
                var detail = currencyVipSystem.Select(o => new VipUpgradeSettingDetail
                {
                    Level = o.Level,
                    LevelValue = type switch
                    {
                        UpgradeSettingEnum.UpBonus => o.UpBonus.ToString(),
                        UpgradeSettingEnum.UpDeposit => o.UpDeposit.ToString(),
                        UpgradeSettingEnum.UpTurnover => o.UpTurnover.ToString(),
                        UpgradeSettingEnum.DownDeposit => o.DownDeposit?.ToString(),
                        UpgradeSettingEnum.DownTurnover => o.DownTurnover?.ToString(),
                        _ => null
                    }
                }).ToList();

                if (siteId != 101)
                {
                    //Leve1跟6沒有提供設定,這邊寫死資料讓前端顯示用
                    detail.Add(new VipUpgradeSettingDetail { Level = 1, LevelValue = null });
                    detail.Add(new VipUpgradeSettingDetail { Level = 6, LevelValue = "By Invitation" });
                }
                
                var setting = _vipLevelSettingList
                    .Where(x => x.Type == type && (x.SiteId == siteId || x.SiteId == 0))
                    .OrderByDescending(x => x.SiteId)
                    .FirstOrDefault();

                if (setting != null)
                    result.Add(new VipUpgradeSetting()
                    {
                        SiteId = siteId,
                        CurrencyId = currencyId,
                        Type = setting.Type,
                        TypeName = setting.TypeName,
                        Detail = detail.OrderBy(y => y.Level)
                    });
            }
            
            return result.OrderBy(x => x.Type);
        }

        public async Task<int> SetUpgradeSetting(ParamsWithLog<VipUpgradeSetting> input)
        {
            return await _vipLevelService.SetUpgradeSetting(input);
        }

        public async Task<IEnumerable<int>> GetVipUpgradeSupportCurrency(int siteId)
        {
            var vipSystem = await _vipLevelService.GetVipSystem(siteId);
            return vipSystem.GroupBy(y => y.CurrencyId).Select(x => x.Key);
        }

        public async Task<IEnumerable<VipLevelName>> GetVipLevelName(int siteId)
        {
            return await _vipLevelService.GetVipLevelName(siteId);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// 判斷是否可以異動資料狀態
        /// </summary>
        /// <param name="calculateYearMonth"></param>
        /// <param name="siteId"></param>
        /// <param name="vipType"></param>
        /// <returns></returns>
        private bool CheckCanUpdateStatus(string calculateYearMonth, int siteId, VipSettingStatusEnum vipType)
        {
            var siteVipSetting = _vipLevelService.GetVipSetting((int)vipType, siteId);
            if (siteVipSetting == null)
                return false;

            var currentTime = DateTime.UtcNow.AddHours(8);
            var previousDateTime = currentTime.AddMonths(-1);
            var previousThreeMonthDateTime = currentTime.AddMonths(-3);
            var currentDay = currentTime.Day;
            string canUpdateYearMonth;

            switch (siteVipSetting.PreviousDateRangeType)
            {
                case (int)PreviousDateRangeTypeEnum.LastMonth:

                    canUpdateYearMonth = new DateTime(previousDateTime.Year, previousDateTime.Month, 1).Date.ToString("yyyyMM");

                    return canUpdateYearMonth == calculateYearMonth && currentDay <= siteVipSetting.AssignSettleDate;

                case (int)PreviousDateRangeTypeEnum.LastThreeMonth:

                    canUpdateYearMonth = new DateTime(previousThreeMonthDateTime.Year, previousThreeMonthDateTime.Month, 1).Date.ToString("yyyyMM")
                                         + new DateTime(previousDateTime.Year, previousDateTime.Month, 1).Date.ToString("yyyyMM");

                    return canUpdateYearMonth == calculateYearMonth && currentDay <= siteVipSetting.AssignSettleDate;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 取得VIP升級檢視清單
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="calculateYearMonth"></param>
        /// <param name="status"></param>
        /// <param name="userName"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalPages"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        private IEnumerable<VipUpgradeReviewDataModel> GetVipUpgradeReview(int siteId,
            string calculateYearMonth,
            string selectStatus,
            string userName,
            int pageNumber,
            int pageSize,
            string currencyIds,
            out int totalPages,
            out int rowCount)
        {
            return _vipLevelService.GetVipUpgradeReviewList(siteId,
                calculateYearMonth,
                selectStatus,
                string.IsNullOrEmpty(userName) ? null : userName,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                currencyIds,
                out totalPages,
                out rowCount);
        }

        /// <summary>
        /// 更新VIP升級檢視資料
        /// </summary>
        /// <param name="custIdList"></param>
        /// <param name="calculateYearMonth"></param>
        /// <param name="newStatus"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        private bool UpdateVipUpgradeReview(IEnumerable<int> custIdList, string calculateYearMonth, VipStatus newStatus, string updater)
        {
            return _vipLevelService.UpdateVipUpgradeReviewFlow(JsonConvert.SerializeObject(custIdList), calculateYearMonth, (int)newStatus, updater);
        }

        /// <summary>
        /// 取得VIP降級檢視清單
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="calculateYearMonth"></param>
        /// <param name="status"></param>
        /// <param name="userName"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalPages"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        private IEnumerable<VipDowngradeReviewDataModel> GetVipDowngradeReview(int siteId,
            string calculateYearMonth,
            string selectStatus,
            string currencyId,
            string userName,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            return _vipLevelService.GetVipDowngradeReviewList(siteId,
                calculateYearMonth,
                selectStatus,
                currencyId,
                string.IsNullOrEmpty(userName) ? null : userName,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }

        /// <summary>
        /// 更新VIP降級檢視資料
        /// </summary>
        /// <param name="custIdList"></param>
        /// <param name="calculateYearMonth"></param>
        /// <param name="newStatus"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        private bool UpdateVipDowngradeReview(IEnumerable<int> custIdList, string calculateYearMonth, VipStatus newStatus, string updater)
        {
            return _vipLevelService.UpdateVipDowngradeReviewFlow(JsonConvert.SerializeObject(custIdList), calculateYearMonth, (int)newStatus, updater);
        }

        private async Task CreateAdminLog(string @operator, int siteId, int currencyId, string beforeLog, string afterLog, ActionType action, int custId, string message, AdminAdjustmentLogMenuEnum menuEnum = AdminAdjustmentLogMenuEnum.SetCustomerInfo)
        {
            var adminLog = new AdminLog()
            {
                Operator = @operator,
                SiteId = siteId,
                CurrencyId = currencyId,
                BeforeDetailInfo = beforeLog,
                AfterDetailInfo = afterLog,
                ActionType = action,
                MenuEnum = menuEnum,
                CustId = custId,
                Menu = "CustManage/CustInfoList",
                Note = message,
                DetailInfo = JsonConvert.SerializeObject(new
                {
                    Action = message,
                    MenuName = "Member Setting",
                })
            };
            try
            {
                await _adminLogService.Insert(adminLog);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "InsertAdminLog failed. params: " + JsonConvert.SerializeObject(adminLog));
            }
        }
        #endregion
    }
}
