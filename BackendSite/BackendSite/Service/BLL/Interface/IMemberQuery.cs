using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL.Interface;

public interface IMemberQuery
{
    Task<GetMemberListPageData<MemberList>> GetListAsync(GetTempMemberListRequest request);
    Task<IEnumerable<MemberExportData>> GetExportDataAsync(ExportMemberRequest request);
    Task<CustomerBasicInfo> GetCustomerBasicInfoAsync(AuthData authData, int custId);
    Task<IEnumerable<MemberLoginDevice>> GetDeviceListByCustIdAsync(int custId);
    Task<IEnumerable<MemberLoginDeviceFPS>> GetFPSDeviceListByCustIdAsync(int custId);
    Task<IEnumerable<CustomerLoginIp>> GetIpListByCustIdAsync(int custId);
    Task<GetLoginLogResponse> GetLoginLogAsync(int custId, DateTime startDate, DateTime endDate, int pageSize, int pageNumber);
    Task<CustomerBalanceInfo> GetCustomerBalanceInfoAsync(int custId);
    Task<IEnumerable<CustomerAssociateBankInfo>> GetCustomerAssociateBankUserAsync(int custId);
    Task<IEnumerable<CustRealNameChangeLog>> GetCustRealNameChangeLogAsync(int custId);
    Task<CustPromotionWhiteList> GetCustPromotionWhiteListAsync(int custId);
    Task<IEnumerable<PromotionReceiveFailLog>> GetPromotionReceiveFailLogAsync(int custId, DateTime startTime, DateTime endTime);
    Task<IEnumerable<CustTpGameTypeSettingForFrontendUse>> GetCustTpGameTypeSettingAsync(int custId, IEnumerable<CustTpGameTypeSettingGetType> getDataTypes);
}