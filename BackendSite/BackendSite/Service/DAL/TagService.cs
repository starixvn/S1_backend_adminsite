using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class TagService : ITagService
    {
        private readonly MySqlClient _reportDbClient;

        public TagService(IEnumerable<MySqlClient> dbClients)
        {
            _reportDbClient = dbClients.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }

        public async Task<IEnumerable<TagDto>> GetAllTag()
        {
            return await _reportDbClient.QueryAsync<TagDto>("Admin_Tag_GetAll");
        }
    }
}