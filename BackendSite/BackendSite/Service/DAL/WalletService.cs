using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using MySql.Data.MySqlClient;

namespace BackendSite.Service.DAL
{
    public class WalletService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public WalletService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public int SetBalance(int custId, int siteId, string userName, decimal Amount, int type, string Memo, string RefTransId, out decimal afterAmount, out byte is1stDepoist, MySqlTransaction trans)
        {
            var p = new DynamicParameters(new { CustId = custId, SiteId = siteId, UserName = userName, Amount = Amount, Type = type, Memo = Memo, RefTransId = RefTransId });
            p.Add("Is1stDepoist", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            p.Add("AfterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            _mainDbClient.Execute("Wallet_Set_V2", trans, p);
            is1stDepoist = p.Get<byte?>("Is1stDepoist") ?? 0;
            afterAmount = p.Get<decimal?>("AfterAmount") ?? 0;
            return afterAmount >= 0 ? 1 : 0;
        }
        public int AdjustmentSetBalance(int custId, int siteId, string userName, decimal amount, int type, string memo, string refTransId,int reasonId, out decimal afterAmount, out byte is1StDeposit, MySqlTransaction trans)
        {
            var p = new DynamicParameters(new { CustId = custId, SiteId = siteId, UserName = userName, Amount = amount, Type = type, Memo = memo, RefTransId = refTransId , ReasonId = reasonId });
            p.Add("Is1stDepoist", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            p.Add("AfterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            _mainDbClient.Execute("Service_AdjustWallet_Set", trans, p);
            is1StDeposit = p.Get<byte?>("Is1stDepoist") ?? 0;
            afterAmount = p.Get<decimal?>("AfterAmount") ?? 0;
            return afterAmount >= 0 ? 1 : 0;
        }
        public decimal GetBalance(int custId)
        {
            return _mainDbClient.ExecuteScalar<decimal>("Balance_Get", new { CustId = custId });
        }
        public Task<decimal> GetBalanceAsync(int custId)
        {
            return _mainDbClient.ExecuteScalarAsync<decimal>("Balance_Get", new { CustId = custId });
        }

        public IEnumerable<BonusLog> GetBonusLog(int custId, string promoCategoryIds, DateTime? startDate, DateTime? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            totalPages = 0;
            var p = new DynamicParameters(
                new
                {
                    CustId = custId,
                    PromoCategoryIds = promoCategoryIds,
                    StartDate = startDate,
                    EndDate = endDate,
                    RefTransId = refTransId,
                    PageSize = pageSize
                });
            p.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            p.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            p.Add("TotalPages", Math.Abs(totalPages), DbType.Int32, ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var depositList = _mainDbClient.Query<BonusLog>("Admin_BonusLogList_Get_V4", p);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }
        public IEnumerable<PromotionCategory> GetPromotionCategory()
        {
            var result = _reportDbClient.Query<PromotionCategory>("Common_PromotionCategory_Get");
            return result;
        }

        public VipRebateReportResult GetVipRebateInfoList(string multiSiteRequest,
            DateTime startDate,
            DateTime endDate,
            string userName,
            string categoryIdStr,
            int baseCurrencyId, int payoutStatuses, int startIndex, int pageSize,
            ref List<string> currencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();
            var searchData = new VipRebateReportResult();
            var props = new DynamicParameters();
            props.Add("_MultiSiteRequest", multiSiteRequest);
            props.Add("_UserName", userName);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_CategoryIds", categoryIdStr);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_PayoutStatus", payoutStatuses);

            #region GetCurrencyTotalAmount

            searchData.CurrencyAmount = _reportDbClient.Query<VipRebateReportOverviewResult>("Admin_VipRebateTotalAmount_Get_V7", props);

            if (searchData.CurrencyAmount.Count() > 0)
            {
                searchData.CurrencyAmount.ToList().ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add($"{x.CurrencyName}:{Math.Round(x.TotalAmount, 2)}");
                });
            }
            currencyAmount = tempCurrencyTotalAmount;

            #endregion

            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<VipRebateInfo>("Admin_VipRebateReportList_Get_V10", props).ToList();

            searchData.VipRebateInfoList = result;
            searchData.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return searchData;
        }
        public VipRebateInfoData GetVipRebateInfoListByCustId(int custId, DateTime? startDate, DateTime? endDate, int status, int pageSize, int pageNumber)
        {
            var searchData = new VipRebateInfoData();
            var tempCurrencyTotalAmount = new List<string>();
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("StartDate", startDate);
            //if (endDate != null) { props.Add("EndDate", endDate.Value.AddDays(1).AddSeconds(-1)); }
            //else { props.Add("EndDate"); }
            props.Add("EndDate", endDate);
            props.Add("PayoutStatus", status);
            #region GetCurrencyTotalAmount

            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_VipRebateTotalAmountbyCust_Get_V4", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            searchData.CurrencyAmount = tempCurrencyTotalAmount;

            #endregion

            props.Add("PageSize", Math.Abs(pageSize));
            props.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32, ParameterDirection.Input);
            props.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<VipRebateInfo>("Admin_VipRebateReportListbyCust_Get_V4", props).ToList();

            searchData.VipRebateInfoList = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return searchData;
        }

        public IEnumerable<VipRebateDetail> GetVipRebateDetail(int custId, int tpCategoryId, string refId)
       => _reportDbClient.Query<VipRebateDetail>("Admin_VipRebateDetail_Get",
           new DynamicParameters(
               new
               {
                   _CustId = custId,
                   _CategoryId = tpCategoryId,
                   _RefId = refId
               }));

        public VipRebateInfoData GetVipRebateInfoListExport(string multiSiteRequest,
            DateTime startDate,
            DateTime endDate,
            string userName,
            string categoryIdStr,
            int baseCurrencyId,
            int spPayoutStatus)
        {
            var searchData = new VipRebateInfoData();
            var props = new DynamicParameters();
            props.Add("_MultiSiteRequest", multiSiteRequest);
            props.Add("_UserName", userName);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_CategoryIds", categoryIdStr);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_PayoutStatus", spPayoutStatus);
            var result = _reportDbClient.Query<VipRebateInfo>("Admin_VipRebateReportListExport_Get_V8", props).ToList();

            searchData.VipRebateInfoList = result;
            searchData.PageNumber = 0;
            searchData.TotalPages = 0;
            searchData.RowCount = 0;
            return searchData;
        }
    }
}
