using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.CustVipLevel;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class VipLevelBLL : IVipLevelBLL
    {
        private readonly VipLevelService _vipLevelService;

        public VipLevelBLL(VipLevelService vipLevelService)
        {
            _vipLevelService = vipLevelService;
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
                VipLevel = data.VipLevel,
                QualifiedTime = data.QualifiedTime
            };;

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

        public int SetVipLevel(int siteId, int custId, string userName, int vipLevel, string updator)
        {
            return _vipLevelService.SetVipLevel(siteId, custId, userName, vipLevel, $"{updator} Update VipLevel",
                updator);
        }
        
        public async Task<int> SetVipLevelAsync(int siteId, int custId, string userName, int vipLevel, string updator)
        {
            return await _vipLevelService.SetVipLevelAsync(siteId, custId, userName, vipLevel, $"{updator} Update VipLevel", updator);
        }

        public int SetVipLevelManually(int siteId, int custId, string userName, int manually, string updator)
        {
            return _vipLevelService.SetVipLevelManually(siteId, custId, userName, manually,
                $"{updator} Update Manually", updator);
        }

        public async Task<int> SetVipLevelManuallyAsync(int siteId, int custId, string userName, int manually, string updator)
        {
            return await _vipLevelService.SetVipLevelManuallyAsync(siteId, custId, userName, manually, $"{updator} Update Manually", updator);
        }
        // public VipUpgradeReviewData GetVipUpgradeReview(string yearMonth, int siteId, int pageNumber, int pageSize,
        //     int status, string userName)
        // {
        //     var vipUpgradeReviewData =
        //         _vipLevelService.GetVipUpgradeReview(yearMonth, siteId, pageNumber, pageSize, status, userName);
        //     vipUpgradeReviewData.VipUpgradeList.ForEach(x =>
        //     {
        //         x.CalculateTime = Convert.ToDateTime(x.CalculateTime).ToString("yyyy/MM/dd HH:mm:ss");
        //         x.SettleTime = string.IsNullOrWhiteSpace(x.SettleTime)
        //             ? x.SettleTime
        //             : Convert.ToDateTime(x.SettleTime).ToString("yyyy/MM/dd HH:mm:ss");
        //     });
        //     return vipUpgradeReviewData;
        // }

        // public IEnumerable<VipUpgradeExport> GetVipUpgradedata(int siteId, string yearMonth, int status,
        //     string userName)
        // {
        //     var exportedDatas = _vipLevelService.GetVipUpgradedata(siteId, yearMonth, status, userName);
        //     exportedDatas.ToList().ForEach(x =>
        //     {
        //         x.CalculateTime = Convert.ToDateTime(x.CalculateTime).ToString("yyyy/MM/dd HH:mm:ss");
        //         x.SettleTime = string.IsNullOrWhiteSpace(x.SettleTime)
        //             ? x.SettleTime
        //             : Convert.ToDateTime(x.SettleTime).ToString("yyyy/MM/dd HH:mm:ss");
        //     });
        //     return exportedDatas;
        // }

        //public bool CanUpgradeVip(string month)
        //{
        //    var currentTime = DateTime.UtcNow.AddHours(8);
        //    var previousDateTime = currentTime.AddMonths(-1);
        //    var CanUpgradeMonth =
        //        new DateTime(previousDateTime.Year, previousDateTime.Month, 1).Date.ToString("yyyyMM");
        //    var day = currentTime.Day;
        //    //只有當月的5號前，可以更新上個月的VipUpgradeReview狀態
        //    return CanUpgradeMonth == month && day <= 5;
        //}

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

        public async Task<IEnumerable<VipUpgradeSetting>> GetUpgradeSetting(int siteId, int CurrencyId)
        {
            var vipLevel = await _vipLevelService.GetVipLevelSettingAsync(siteId);
            var vipSystem = await _vipLevelService.GetVipSystem(siteId);
            var result = new List<VipUpgradeSetting>();
            foreach (var vip in vipLevel)
            {
                result.Add(new VipUpgradeSetting()
                {

                });
            }

            return result;
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
            var siteVipSetting = GetVipSettings(vipType).FirstOrDefault(x => x.SiteId == siteId);
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
        /// 取得VIP排程設定資料
        /// </summary>
        /// <param name="vipType"></param>
        /// <returns></returns>
        private IEnumerable<VipSettingModel> GetVipSettings(VipSettingStatusEnum vipType)
        {
            return _vipLevelService.GetVipSetting((int)vipType);
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

        #endregion
    }
}
