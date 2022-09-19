using BackendSite.Service.Model;
using Dapper;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class ReasonManagementService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        
        public ReasonManagementService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<ReasonManagementFuncKey> GetFuncKey()
        {
            return _reportDbClient.Query<ReasonManagementFuncKey>("Admin_BankFinancialReason_FuncKey_Get");
        }

        public GetReasonManagementDataResult GetReasonManagementData(GetReasonManagementDataPara para)
        {
            var searchResult = new GetReasonManagementDataResult();
            
            var props = new DynamicParameters();
            props.Add("_FuncKeys", JsonConvert.SerializeObject(para.FuncKeys));
            props.Add("_PageSize", Math.Abs(para.PageSize));
            props.Add("_StartIndex", (Math.Abs(para.PageNumber) - 1) * Math.Abs(para.PageSize));
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<ReasonManagementData>("Admin_BankFinancialReason_Get_V2", props);

            searchResult.ReasonManagementData = result.ToList();
            searchResult.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            searchResult.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return searchResult;
        }

        public int SetReasonManagement(SetReasonManagementPara para, int userId)
        {
            var props = new DynamicParameters();
            props.Add("_Id", para.Id);
            props.Add("_Description", para.Description);
            props.Add("_Updator", userId);

            return _mainDbClient.Execute("Admin_BankFinancialReason_Upd_V2", props);
        }
        public int InsertReasonManagement(InsertReasonManagementPara para, int userId)
        {
            var props = new DynamicParameters();

            props.Add("_FuncKey", para.FuncKey);
            props.Add("_Description", para.Description);
            props.Add("_Updator", userId);
            props.Add("_Creator", userId);

            return _mainDbClient.Execute("Admin_BankFinancialReason_Ins_V2", props);
        }
    }
}
