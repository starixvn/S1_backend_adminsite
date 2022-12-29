using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IIovationReportBLL
{
    Task<IovationReportPageResult<IovationReport>> GetListAsync(IovationReportGetListRequest request);
    Task<IEnumerable<IovationReport>> GetListWithoutPageAsync(IovationReportExportRequest request);
}