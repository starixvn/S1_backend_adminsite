using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;
using System.Linq;

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
            VipUpgradeReviewData.VipUpgradeList.ForEach(x => { 
                x.CalculateTime = Convert.ToDateTime(x.CalculateTime).ToString("yyyy/MM/dd HH:mm:ss");
                x.SettleTime = string.IsNullOrWhiteSpace(x.SettleTime) ? x.SettleTime : Convert.ToDateTime(x.SettleTime).ToString("yyyy/MM/dd HH:mm:ss");
            });
            return VipUpgradeReviewData;
        }
        public int UpdateVipUpgradeReview(string CustIds, string YearMonth,string username,int ChangeStatus)
        {
            return vipLevelService.UpdateVipUpgradeReview(CustIds, YearMonth, username, ChangeStatus);
        }

        public IEnumerable<VipUpgradeExport> GetVipUpgradedata(int SiteId, string YearMonth, int Status, string UserName)
        {
            var exportdata = vipLevelService.GetVipUpgradedata(SiteId, YearMonth, Status, UserName);
            exportdata.ToList().ForEach(x =>
            {
                x.CalculateTime = Convert.ToDateTime(x.CalculateTime).ToString("yyyy/MM/dd HH:mm:ss");
                x.SettleTime = string.IsNullOrWhiteSpace(x.SettleTime) ? x.SettleTime : Convert.ToDateTime(x.SettleTime).ToString("yyyy/MM/dd HH:mm:ss");
            });
            return exportdata;
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
