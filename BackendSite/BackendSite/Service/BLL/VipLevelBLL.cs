using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class VipLevelBLL
    {
        private readonly VipLevelService vipLevelService;

        public VipLevelBLL(VipLevelService vipLevelService)
        {
            this.vipLevelService = vipLevelService;
        }

        public IEnumerable<VipLevelSetting> GetVipLevelSetting(int SiteId)
        {
            return vipLevelService.GetVipLevelSetting(SiteId);
        }
        public VipLevelInfo GetVipLevelInfo(int CustId)
        {
            return vipLevelService.GetVipLevelInfo(CustId);
        }
        public IEnumerable<VipLevelLog> GetVipLevelLog(int CustId)
        {
            return vipLevelService.GetVipLevelLog(CustId);
        }
        public int SetVipLevel(int SiteId, int CustId, string UserName, int VipLevel, string Operator)
        {
            return vipLevelService.SetVipLevel(SiteId, CustId, UserName, VipLevel, $"{Operator} Update VipLevel", Operator);
        }
        public int SetVipLevelManually(int SiteId, int CustId, string UserName, int Manually, string Operator)
        {
            return vipLevelService.SetVipLevelManually(SiteId, CustId, UserName, Manually, $"{Operator} Update Manually", Operator);
        }
        public VipUpgradeReviewData GetVipUpgradeReview(string YearMonth, int siteid, int PageNumber, int PageSize, int Status, string UserName)
        {
            var VipUpgradeReviewData = vipLevelService.GetVipUpgradeReview(YearMonth, siteid, PageNumber, PageSize, Status, UserName);
            var VipSystemData = vipLevelService.GetVipSystemData(siteid);
            VipUpgradeReviewData.VipUpgradeList.ForEach(x => x.ThresholdAmount = VipSystemData.Find(y => y.Level == x.UpgradeVipLevel && y.CurrencyId == x.CurrencyId).UpDeposit);
            VipUpgradeReviewData.VipUpgradeList.ForEach(x => x.CalculateTime = Convert.ToDateTime(x.CalculateTime).ToString("yyyy/MM/dd HH:mm:ss"));
            return VipUpgradeReviewData;
        }
        public int UpdateVipUpgradeReview(string CustIds, string YearMonth)
        {
            return vipLevelService.UpdateVipUpgradeReview(CustIds, YearMonth);
        }

        public enum VipStatus
        {
            Pending = 1,
            Approved = 2,
            Rejected = 3
        }

        public IEnumerable<VipUpgradeExport> GetVipUpgradedata(int SiteId, string YearMonth, int Status, string UserName)
        {
            List<VipUpgradeExport> VipUpgradeExport = new List<VipUpgradeExport>();
            var result = vipLevelService.GetVipUpgradedata(SiteId, YearMonth, Status, UserName);
            var VipSystemData = vipLevelService.GetVipSystemData(SiteId);
            result.ForEach(x => {
                VipUpgradeExport.Add(new VipUpgradeExport
                {
                    SiteName = x.SiteName,
                    UserName = x.UserName,
                    CurrentVipLevel = x.CurrentVipName,
                    UpgradeVipLevel = x.UpgradeVipName,
                    Currency = x.CCY,
                    DepositAmount = x.FullAmount,
                    ThresholdAmount = VipSystemData.Find(y => y.Level == x.UpgradeVipLevel && y.CurrencyId == x.CurrencyId).UpDeposit,
                    UpgradeBonus = x.UpgradeAmount,
                    CalculateTime = x.CalculateTime,
                    Status = Enum.GetName(typeof(VipStatus), x.Status)
                });
            });
            return VipUpgradeExport;
        }
        public bool CanUpgradeVip(string month)
        {
            var currentTime = DateTime.UtcNow.AddHours(8);
            var previousDateTime = currentTime.AddMonths(-1);
            var CanUpgradeMonth = new DateTime(previousDateTime.Year, previousDateTime.Month, 1).Date.ToString("yyyyMM");
            var day = currentTime.Day;
            //只有當月的5號前，可以更新上個月的VipUpgradeReview狀態
            return CanUpgradeMonth == month && day <=5;
        }
    }
}
