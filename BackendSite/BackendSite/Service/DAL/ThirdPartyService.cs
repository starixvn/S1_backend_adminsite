using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpMaintain;

using Dapper;

using TpGameType = BackendSite.Service.Model.TpGameType;

namespace BackendSite.Service.DAL
{
    public class ThirdPartyService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public ThirdPartyService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }
        public IEnumerable<TpWallet> GetTpWallet(int custId)
        {
            return _reportDbClient.Query<TpWallet>("TpWallet_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<TpWallet>> GetTransferTpWallet(int custId)
        {
            var p = new DynamicParameters(new
            {
                _CustId = custId
            });
            return await _reportDbClient.QueryAsync<TpWallet>("Admin_TransferTpWalletInfo_Get", p);
        }

        public IEnumerable<TpTransferInfo> GetTpTransferInfoByCust(int CustId,
            string TransId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", CustId, dbType: DbType.Int32);
            p.Add("_TransId", TransId, dbType: DbType.String);
            p.Add("_StartDate", startDate.DateTime, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate.DateTime, dbType: DbType.DateTime);
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var tpTransferInfoList = _reportDbClient.Query<TpTransferInfo>("Admin_TpTransferInfoByCust_Get_V4", p, commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return tpTransferInfoList;
        }
        public IEnumerable<SiteTpListIfo> GetSiteTpList(int siteId)
        {
            return _mainDbClient.Query<SiteTpListIfo>("Admin_SiteTpList_Get", new DynamicParameters(new { SiteId = siteId }));
        }

        public Task<IEnumerable<SiteTpListIfo>> GetSiteTpListAllAsync(int siteId)
        {
            var para = new DynamicParameters();
            para.Add("_SiteId", siteId);

            return _reportDbClient.QueryAsync<SiteTpListIfo>("Admin_SiteTpList_GetAll", para);
        }

        public IEnumerable<AdminTpTransferInfo> GetAdjustmentTpLog(int siteId, int tpId, int custId, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("@SiteId", siteId);
            props.Add("@TpId", tpId);
            props.Add("@CustId", custId);
            props.Add("@StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("@PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@PageSize", Math.Abs(pageSize));

            var result = _mainDbClient.Query<AdminTpTransferInfo>("Admin_LogAdminAdjustWallet_Get_V2", props);
            pageNumber = props.Get<int?>("@PageNumber") ?? 0;
            totalPages = props.Get<int?>("@TotalPages") ?? 0;
            rowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public IEnumerable<TpAdjustmentLog> GetCustTpAdjustmentLog(GetCustTpAdjustmentLogParams input,
            int pageSize,
            int startIndex,
            out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", input.CustId, dbType: DbType.Int32);
            p.Add("_StartTime", input.StartTime.UtcDateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", input.EndTime.UtcDateTime, dbType: DbType.DateTime);
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _mainDbClient.Query<TpAdjustmentLog>("Admin_CustLogAdminAdjustWallet_Get", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public async Task<TpAdjustmentLogWrapper> GetCustTpAdjustmentLogV2(GetCustTpAdjustmentLogParamsV2 input)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", input.CustId, dbType: DbType.Int32);
            p.Add("_SiteId", input.SiteId, dbType: DbType.Int32);
            p.Add("_TpId", input.TpId, dbType: DbType.Int32);
            p.Add("_Direction", input.Direction, dbType: DbType.Int16);
            p.Add("_StartTime", input.StartTime.UtcDateTime, dbType: DbType.DateTime);
            p.Add("_EndTime", input.EndTime.UtcDateTime, dbType: DbType.DateTime);
            p.Add("_PageSize", input.PageSize, dbType: DbType.Int32);
            p.Add("_StartIndex", input.StartIndex, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = new TpAdjustmentLogWrapper
            {
                Data = await _reportDbClient.QueryAsync<TpAdjustmentLogV2>("Admin_CustLogAdminAdjustWallet_Get_V2", p),
                TotalPages = p.Get<int?>("_TotalPages") ?? 0,
                RowCount = p.Get<int?>("_RowCount") ?? 0
            };

            return result;
        }

        public IEnumerable<SiteTpListIfo> GetTpBySiteAndCurrency(string siteIds, int currency)
        {
            return _reportDbClient.Query<SiteTpListIfo>("Admin_TpList_Get_V2", new { _SiteIds = siteIds, _CurrencyId = currency, }, commandType: CommandType.StoredProcedure);
        }

        public void SetAdjustmentTpCustInfoLog(string menu, int actionType, string detailInfo, string @operator)
        {
            var props = new DynamicParameters();
            props.Add("Menu", menu);
            props.Add("ActionType", actionType);
            props.Add("DetailInfo", detailInfo);
            props.Add("Operator", @operator);

            _mainDbClient.Execute("Admin_AdjustmentInfoLog_Set", props);
        }

        public async Task<TpCurrency> GetTpCurrency(int tpId, int currencyId)
        {
            return await _reportDbClient.QueryFirstAsync<TpCurrency>("TpCurrency_Get", new DynamicParameters(new { TpId = tpId, CurrencyId = currencyId }));
        }

        public async Task<IEnumerable<TpCustInfo>> GetTpCustInfoListByCustId(int custId)
        {
            return await _reportDbClient.QueryAsync<TpCustInfo>("Admin_TpCustInfo_GetListByCustId_V2", new DynamicParameters(new { _CustId = custId }));
        }

        public async Task<IEnumerable<CommonTpInfo>> GetCommonTpList()
        {
            return await _reportDbClient.QueryAsync<CommonTpInfo>("Common_TpList_Get");
        }

        public async Task<IEnumerable<TpGameType>> GetTpGameType()
        {
            return await _reportDbClient.QueryAsync<TpGameType>("Admin_TpGameType_Get");
        }
        public async Task<IEnumerable<TpGameTypeCurrency>> GetTpGameTypeCurrency()
        {
            return await _reportDbClient.QueryAsync<TpGameTypeCurrency>("Admin_TpGameTypeCurrency_Get");
        }

        public async Task<IEnumerable<TpGameType>> GetTpGameTypeListbyCategoryAsync(string siteIds, int currencyId, int categoryId)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteIds);
            p.Add("_CurrencyId", currencyId);
            p.Add("_CategoryId", categoryId);
            return await _reportDbClient.QueryAsync<TpGameType>("Admin_TpGameTypebyCategory_Get_V2", p);
        }

        public async Task<IEnumerable<SiteTpListIfo>> GetTpListByCategoryAsync(int categoryId)
        {
            var p = new DynamicParameters();
            p.Add("_CategoryId", categoryId);
            return await _reportDbClient.QueryAsync<SiteTpListIfo>("Admin_TpListByCategory_Get", p);
        }
    }
}
