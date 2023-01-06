using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetAllTag();
    }
}
