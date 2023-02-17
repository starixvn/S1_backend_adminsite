using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface;

public interface IAdminLogService
{
    Task<int> Insert(AdminLog input);
}