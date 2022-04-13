using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class HotGameBLL
    {
        private readonly HotGameService _hotGameService;

        private Dictionary<int, string> CurrencyLangIdMap = new Dictionary<int, string>()
        {
            { 2, "en" },
            { 4, "th" },
            { 15, "id" },
            { 51, "vn" },
            { 61, "hi" }
        };

        public HotGameBLL(HotGameService hotGameService)
        {
            _hotGameService = hotGameService;
        }
        
        public async Task<IEnumerable<GetHotGameListResponse>> Get(int siteId, int currencyId)
        {
            return await _hotGameService.Get(siteId, currencyId);
        }
        
        public async Task<GetHotGameDetailResponse> Get(int id)
        {
            return await _hotGameService.Get(id);
        }
        
        public async Task<int> CreateOrUpdate(SetHotGameRequest request, string updateOperator)
        {
            request.LangId = CurrencyLangIdMap[request.CurrencyId];
            return await _hotGameService.CreateOrUpdate(request, updateOperator);
        }

        public async Task<int> Delete(int id)
        {
            return await _hotGameService.Delete(id);
        }
    }
}