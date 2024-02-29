using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class PromotionService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public PromotionService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }
        public IEnumerable<Promotion> GetPromotionListByCustId(int custId)
        {
            return _reportDbClient.Query<Promotion>("Admin_PromotionListByCustId_Get_V2", new { _CustId = custId });
        }

        public PromotionListData GetPromotionListByCustIdAndDate(int CustId, DateTimeOffset StartDate, DateTimeOffset EndDate, int PageNumber, int PageSize)
        {
            var PromotionListData = new PromotionListData();
            var tempCurrencyTotalAmount = new List<string>();

            var props = new DynamicParameters();
            props.Add("_CustId", CustId, DbType.Int32);
            props.Add("_StartDate", StartDate.DateTime, DbType.DateTime);
            props.Add("_EndDate", EndDate.DateTime, DbType.DateTime);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_PromotionTotalAmountByCustIdAndDate_Get_V3", props).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            PromotionListData.CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            props.Add("_StartIndex", (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize));
            props.Add("_PageSize", Math.Abs(PageSize));
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<Promotion>("Admin_PromotionListByCustIdAndDate_Get_V4", props).ToList();

            PromotionListData.PromotionList = result;
            PromotionListData.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            PromotionListData.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return PromotionListData;
        }

        public async Task<PromotionResponseWrapper> GetPromotionListByCustIdAndDateAsync(CustIdWithDateRangeAndPageRequest request)
        {
            var result = new PromotionResponseWrapper();
            var tempCurrencyTotalAmount = new List<string>();

            var P = new DynamicParameters();
            P.Add("_CustId", request.CustId, DbType.Int32);
            P.Add("_StartDate", request.StartDate.DateTime, DbType.DateTime);
            P.Add("_EndDate", request.EndDate.DateTime, DbType.DateTime);

            var CurrencyTotalAmount = (await _reportDbClient.QueryAsync<TotalAmount>("Admin_PromotionTotalAmountByCustIdAndDate_Get_V3", P)).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            result.CurrencyAmount = tempCurrencyTotalAmount;

            P.Add("_StartIndex", (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize));
            P.Add("_PageSize", Math.Abs(request.PageSize));
            P.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            P.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = (await _reportDbClient.QueryAsync<PromotionDto>("Admin_PromotionListByCustIdAndDate_Get_V5", P)).ToList();

            result.PromotionList = data;
            result.TotalPages = P.Get<int?>("_TotalPages") ?? 0;
            result.RowCount = P.Get<int?>("_RowCount") ?? 0;

            return result;
        }
    }
}
