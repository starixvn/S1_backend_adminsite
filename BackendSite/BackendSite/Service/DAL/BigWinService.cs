using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class BigWinService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public BigWinService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public IEnumerable<BigWinSetting> GetBigWinSetting(int siteId)
        {
            var p = new DynamicParameters(new
            {
                SiteId = siteId
            });

            return _reportDbClient.Query<BigWinSetting>("Admin_BigWinSetting_Get", p);
        }

        public async Task<bool> SetBigWinSetting(ParamsWithLog<BigWinSetting> input)
        {
            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();

            var p = new DynamicParameters(new
            {
                SiteId = input.SiteId,
                CurrencyId = input.CurrencyId,
                Status = input.After.Status,
                TpCategory = JsonConvert.SerializeObject(input.After.ITpCategoryList),
                MultipleOfWinning = input.After.MultipleOfWinning
            });

            var success = await conn
                .ExecuteAsync("Admin_BigWinSetting_Set", p, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetBigWinSetting);
                logParam.Add("_ActionType", (int)ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = await conn
                    .ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                    return true;
                }
            }

            await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);

            return false;
        }

        public IEnumerable<BigWinReport> GetBigWinReport(IEnumerable<int> currencyId, DateTime startDate, DateTime endDate)
        {
            var p = new DynamicParameters(new
            {
                _CurrencyList = JsonConvert.SerializeObject(currencyId),
                _StartDate = startDate,
                _EndDate = endDate,
            });

            return _reportDbClient.Query<BigWinReport>("Admin_BigWinInfo_Get", p);
        }
    }
}
