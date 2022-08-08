using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface ISocialNetworkService
    {
        Task<IEnumerable<SocialDataListDto>> GetSocialDataList(SocialDataListRequest request);

        Task<int> SetSocialSort(SetSocialSortDto setSocialSort);

        Task<int> SetSocialData(SetSocialDataRequest request,string lastUpdateUser);
    }
}
