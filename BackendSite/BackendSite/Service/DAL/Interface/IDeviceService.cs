using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface IDeviceService
    {
        Task<IEnumerable<CustIovationData>> GetCustIovationList(int custId);
        Task<IEnumerable<CustShieldRiskInfoData>> GetCustShieldRiskInfoList(int custId);
        Task<IEnumerable<CustFPSData>> GetCustFPSList(int custId);

    }
}