using System.Data;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class AdminLogService : IAdminLogService
    {
        private readonly MySqlClient _mainDbClient;

        public AdminLogService(IEnumerable<MySqlClient> dbClients)
        {
            _mainDbClient = dbClients.SingleOrDefault(a=>a.DataBase == DataBase.Main);
        }

        public async Task<int> Insert(AdminLog input)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", input.CurrencyId);
            props.Add("_MenuId", (int) input.MenuEnum);
            props.Add("_ActionType", (int) input.ActionType);
            props.Add("_RefTransId", input.RefTransId ?? string.Empty);
            props.Add("_BeforeDetailInfo", input.BeforeDetailInfo, DbType.String);
            props.Add("_AfterDetailInfo", input.AfterDetailInfo, DbType.String);
            props.Add("_CustId", input.CustId);
            props.Add("_Menu", input.Menu);
            props.Add("_Note", input.Note);
            props.Add("_DetailInfo", input.DetailInfo);
            props.Add("_Operator", input.Operator);
            
            return await _mainDbClient.ExecuteAsync("Admin_AdminLog_Ins", props);
        }
    }
}