using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;

namespace BackendSite.Service.DAL
{
    public class UserBetLimitService
    {
        private readonly MySqlClient _sqlClient;

        public UserBetLimitService(MySqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }

        public async Task<IEnumerable<TpCategoryData>> GetTpCategoryList()
        {
            return await _sqlClient.QueryAsync<TpCategoryData>("Admin_TPCategory_Get", null);
        }
    }
}