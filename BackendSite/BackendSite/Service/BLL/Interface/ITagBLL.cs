using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface
{
    public interface ITagBLL
    {
        Task<IEnumerable<Tag>> GetAllTag();
    }
}