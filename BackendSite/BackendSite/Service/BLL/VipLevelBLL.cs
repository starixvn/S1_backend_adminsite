using BackendSite.Service.DAL;
using BackendSite.Service.Model;
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
    }
}
