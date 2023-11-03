using System.Data;

using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class SocialNetworkService : ISocialNetworkService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public SocialNetworkService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public async Task<IEnumerable<SocialDataListDto>> GetSocialDataList(SocialDataListRequest request)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId, dbType: DbType.Int64);
            p.Add("_CurrencyId", request.CurrencyId, dbType: DbType.Byte);

            return await _reportDbClient.QueryAsync<SocialDataListDto>("Admin_SocialDataList_Get", p);
        }

        public Task<int> SetSocialSort(SetSocialSortDto setSocialSort)
        {
            var p = new DynamicParameters();
            p.Add("_Id", setSocialSort.Id, dbType: DbType.Int64);
            p.Add("_SiteId", setSocialSort.SiteId, dbType: DbType.Int64);
            p.Add("_CurrencyId", setSocialSort.CurrencyId, dbType: DbType.Byte);
            p.Add("_LangId", setSocialSort.LangId, dbType: DbType.String);
            p.Add("_Sort", setSocialSort.Sort, dbType: DbType.Int64);
            p.Add("_LastUpdateUser", setSocialSort.LastUpdateUser, dbType: DbType.String);

            return _mainDbClient.ExecuteAsync("Admin_SocialSort_Set", p);
        }

        public Task<int> SetSocialData(SetSocialDataRequest request, string lastUpdateUser)
        {
            var p = new DynamicParameters();
            p.Add("_Id", request.Id, dbType: DbType.Int64);
            p.Add("_Title", request.Title, dbType: DbType.String);
            p.Add("_MainPageStatus", request.MainPageStatus, dbType: DbType.Boolean);
            p.Add("_ContactPageStatus", request.ContactPageStatus, dbType: DbType.Boolean);
            p.Add("_FooterStatus", request.FooterStatus, dbType: DbType.Boolean);
            p.Add("_AppPageStatus", request.AppPageStatus, dbType: DbType.Boolean);
            p.Add("_Link", request.Link, dbType: DbType.String);
            p.Add("_AndroidLink", request.AndroidLink, dbType: DbType.String);
            p.Add("_iOSLink", request.IosLink, dbType: DbType.String);
            p.Add("_LinkType", request.LinkType, dbType: DbType.Int16);
            p.Add("_LastUpdateUser", lastUpdateUser, dbType: DbType.String);

            return _mainDbClient.ExecuteAsync("Admin_SocialData_Set", p);
        }
    }
}
