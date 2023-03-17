using BackendSite.Service.Model;
using Dapper;
using Newtonsoft.Json;
using System.Data;
using BackendSite.Controllers.ApiV2.CMS.Setting;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class BettingMonitorService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public BettingMonitorService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        
        public IEnumerable<BettingMonitorModel> GetList(
            int siteId, 
            IEnumerable<int> currencyIds, 
            IEnumerable<int> tpIds, 
            IEnumerable<int> categoryIds, 
            IEnumerable<int> gameTypes, 
            int startIndex, int pageSize,
            out int totalPages, out int rowCount)
        {

            var props = new DynamicParameters();

            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            props.Add("_TpIds", JsonConvert.SerializeObject(tpIds));
            props.Add("_CategoryIds", JsonConvert.SerializeObject(categoryIds));
            props.Add("_GameTypes", JsonConvert.SerializeObject(gameTypes));
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<BettingMonitorModel>("Admin_BettingMonitorSetting_Get", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
        
        public bool SetSetting(ParamsWithLog<BettingMonitorController.SetRequest> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _TpId = input.After.TpId,
                _GameType = input.After.GameType,
                _Frequency = input.After.Frequency,
                _Status = input.After.Status,
                _Operator = input.Operator
            };
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BettingMonitor);
            logParam.Add("_ActionType", input.Before.GameType == 0 ? (int) ActionType.Create : (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
            logParam.Add("_Operator", input.Operator);

            var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }
            
            var result = _mainDbClient.Execute("Admin_BettingMonitorSetting_Set", tran, param) > 0;
            if (!result)
            {
                tran.Rollback();
                return false;
            }
            
            tran.Commit();

            return true;
            
        }
        
        public bool DelSetting(ParamsWithLog<BettingMonitorController.BaseSetting> input)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _TpId = input.Before.TpId,
                _GameType = input.Before.GameType,
                _Operator = input.Operator
            };
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BettingMonitor);
            logParam.Add("_ActionType", (int) ActionType.Delete);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", null , DbType.String);
            logParam.Add("_Operator", input.Operator);

            var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", tran, logParam) > 0;
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }
            
            var result = _mainDbClient.Execute("Admin_BettingMonitorSetting_Del", tran, param) > 0;
            if (!result)
            {
                tran.Rollback();
                return false;
            }
            
            tran.Commit();

            return true;
            
        }
    }
}
