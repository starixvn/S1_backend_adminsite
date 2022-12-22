using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface IMarketingService
{
    Task<IEnumerable<PointLevelInfo>> GetPointLevelInfos(int siteId);
}