using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class WalletService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public WalletService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public int SetBalance(int custId, int siteId, string userName, decimal Amount, int type, string Memo, string RefTransId, out decimal afterAmount, out byte is1stDepoist, MySqlConnection conn, MySqlTransaction trans)
        {
            var p = new DynamicParameters(new { CustId = custId, SiteId = siteId, UserName = userName, Amount = Amount, Type = type, Memo = Memo, RefTransId = RefTransId });
            p.Add("Is1stDepoist", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            p.Add("AfterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            conn.Execute("Wallet_Set", p, transaction: trans, commandType: CommandType.StoredProcedure);
            is1stDepoist = p.Get<byte?>("Is1stDepoist") ?? 0;
            afterAmount = p.Get<decimal?>("AfterAmount") ?? 0;
            return afterAmount >= 0 ? 1 : 0;
        }
        public decimal GetBalance(int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.ExecuteScalar<decimal>("Balance_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
        }
        public int SetPayDummy(int siteId, int payId, int currencyId, decimal amount, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.Execute("PayDummy_Set", new { SiteId = siteId, PayId = payId, CurrencyId = currencyId, Amount = amount }, trans, commandType: CommandType.StoredProcedure);
        }
        public int SetBankOffDummy(int siteId, int sysId, string accountNo, int currencyId, decimal amount, MySqlConnection conn, MySqlTransaction trans)
        {
            return conn.Execute("BankOffDummy_Set", new { SiteId = siteId, SysId = sysId, AccountNo = accountNo, CurrencyId = currencyId, Amount = amount }, trans, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<FinancialLog> GetFinancialLog(int custId, DateTime? startDate, DateTime? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new
            {
                CustId = custId, 
                StartDate = startDate, 
                EndDate = endDate, 
                RefTransId = refTransId,
                PageSize = Math.Abs(pageSize)
            });
            p.Add("StartIndex",(Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),DbType.Int32, ParameterDirection.Input);
            p.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32,  ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var depositList = _reportDbClient.Query<FinancialLog>("Admin_FinancialLogList_Get_V3",p);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }
        public IEnumerable<BonusLog> GetBonusLog(int custId, int promoCategoryId, DateTime? startDate, DateTime? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            totalPages = 0;
            var p = new DynamicParameters(
                new { CustId = custId, 
                    PromoCategoryId = promoCategoryId,
                    StartDate = startDate, 
                    EndDate = endDate,
                    RefTransId = refTransId,
                    PageSize = pageSize });
            p.Add("StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32,ParameterDirection.Input);
            p.Add("PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            p.Add("TotalPages", Math.Abs(totalPages), DbType.Int32, ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var depositList = _mainDbClient.Query<BonusLog>("Admin_BonusLogList_Get_V3", p);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }
        public IEnumerable<RebateLog> GetRebateLogByCust(int CustId, long? startDate, long? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { CustId = CustId, StartDate = startDate, EndDate = endDate, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var rebateLogList = conn.Query<RebateLog>("Admin_RebateLogListByCust_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return rebateLogList;
        }
        public IEnumerable<RebateLog> GetRebateLog(IEnumerable<int> SelectSite, string userName, int rebateType, long startDate, long endDate, int pageSize, int currencyId, int baseCurrencyId, int rebateCategoriesId, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var p = new DynamicParameters(new { SelectSite = JsonConvert.SerializeObject(SelectSite), UserName = userName, RebateType = rebateType
                , StartDate = startDate, EndDate = endDate, PageSize = pageSize, CategoryType= rebateCategoriesId, CurrencyId= currencyId,BaseCurrencyId= baseCurrencyId});

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_RebateLogTotalAmountV2_Get", p, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var rebateLogList = conn.Query<RebateLog>("Admin_RebateLogListV3_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;

            return rebateLogList;
        }
        public IEnumerable<RebateLog> GetRebateLogExport(string SelectSite, string userName, int rebateType, long startDate, long endDate, int currencyId, int baseCurrencyId, int rebateCategoriesId)
        {
            var p = new DynamicParameters(new { SelectSite, UserName = userName, RebateType = rebateType, StartDate = startDate, EndDate = endDate, CategoryType = rebateCategoriesId, CurrencyId = currencyId, BaseCurrencyId = baseCurrencyId });
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var rebateLogList = conn.Query<RebateLog>("Admin_RebateLogListExportV3_Get", p, commandType: CommandType.StoredProcedure);
            return rebateLogList;
        }
        public IEnumerable<RebateInfo> GetRebateInfoByCust(int CustId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var rebateInfoList = conn.Query<RebateInfo>("Admin_RebateInfoByCust_Get_V2", new { CustId }, commandType: CommandType.StoredProcedure);
            return rebateInfoList;
        }
        public IEnumerable<RebateInfo> GetRebateInfoList(IEnumerable<int> selectSite, string userName, int rebateType, long startDate, long endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var abc= JsonConvert.SerializeObject(selectSite);

            var p = new DynamicParameters(new { SelectSite = JsonConvert.SerializeObject(selectSite), UserName = userName, RebateType = rebateType, StartDate = startDate, EndDate = endDate, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var rebateInfoList = conn.Query<RebateInfo>("Admin_RebateInfoList_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return rebateInfoList;
        }
        public IEnumerable<RebateInfo> GetRebateInfoExport(string SelectSite, string userName, int rebateType, long startDate, long endDate)
        {
            var p = new DynamicParameters(new { SelectSite, UserName = userName, RebateType = rebateType, StartDate = startDate, EndDate = endDate });
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var rebateInfoList = conn.Query<RebateInfo>("Admin_RebateInfoListExport_Get", p, commandType: CommandType.StoredProcedure);
            return rebateInfoList;
        }

        public IEnumerable<PromotionCategory> GetPromotionCategory()
        {
            var result = _reportDbClient.Query<PromotionCategory>("Common_PromotionCategory_Get");
            return result;
        }
        public IEnumerable<TpBetRecordListOneWeek> GetCustTpBetRecordListOneWeek(int custId)
        {
            var result = _reportDbClient.Query<TpBetRecordListOneWeek>("Admin_TpBetRecord_byCustIDOneWeek_Get", new DynamicParameters(new { CustId = custId }));
            return result;
        }
        public IEnumerable<TpBetRecordListOneDay> GetCustTpBetRecordListOneDay(int custId, DateTime selDate, DateTime startDate, DateTime endDate)
        {
            var result = _reportDbClient.Query<TpBetRecordListOneDay>("Admin_TpBetRecord_byOneDay_Get_V5", new DynamicParameters(new
            {
                CustId = custId, 
                SelDate = selDate,
                StartDate = startDate, 
                EndDate = endDate
            }));
            return result;
        }
        public IEnumerable<TpBetRecordbyCategoryId> GetCustTpBetRecordListbyCategoryId(int custId, DateTime selDate, int categoryId, DateTime startDate, DateTime endDate)
        {
            var result = _reportDbClient.Query<TpBetRecordbyCategoryId>("Admin_TpBetRecord_byCategoryId_Get_V5", new DynamicParameters(new  { CustId = custId, SelDate = selDate, CategoryId = categoryId, StartDate = startDate, EndDate = endDate }));
            return result;
        }
        public IEnumerable<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(int custId, DateTime selDate, int gameTypeId, DateTime startDate, DateTime endDate, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount)
        {
            var props = new DynamicParameters(new { CustId = custId, SelDate = selDate, TypeId = gameTypeId, StartDate = startDate, EndDate = endDate });
            props.Add("@PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),DbType.Int32,ParameterDirection.Input);
            props.Add("@PageSize", Math.Abs(pageSize));

            var result = _reportDbClient.Query<TpBetRecordDetailbyGameType>("Admin_TpBetRecord_byGameTypeList_Get_V5", props);
            pageNumber = props.Get<int?>("@PageNumber") ?? 0;
            totalPages = props.Get<int?>("@TotalPages") ?? 0;
            rowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public VipRebateInfoData GetVipRebateInfoList(IEnumerable<int> selectSite, int currency, long startDate, long endDate, string User, int pageSize, int pageNumber,int CategoryId, int BaseCurrencyId,ref List<string> CurrencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();
            var SearchData = new VipRebateInfoData();
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            props.Add("SelectSite", JsonConvert.SerializeObject(selectSite));
            props.Add("Currency", currency);
            props.Add("UserName", string.IsNullOrWhiteSpace(User) ? null : User);
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("PageSize", pageSize);
            props.Add("CategoryId", CategoryId);
            props.Add("BaseCurrencyId", BaseCurrencyId);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_VipRebateTotalAmountV4_Get", props, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            props.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<VipRebateInfo>("Admin_VipRebateReportListV7_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.VipRebateInfoList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return SearchData;
        }
        public VipRebateInfoData GetVipRebateInfoListByCustId(int custId, DateTime? startDate, DateTime? endDate, int pageSize, int pageNumber)
        {
            var searchData = new VipRebateInfoData();
            var tempCurrencyTotalAmount = new List<string>();
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("StartDate", startDate);
            if (endDate != null) { props.Add("EndDate", endDate.Value.AddDays(1).AddSeconds(-1)); }
            else { props.Add("EndDate"); }

            #region GetCurrencyTotalAmount
            
            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_VipRebateTotalAmountbyCust_Get_V3", props).ToList();
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
            var result = _reportDbClient.Query<VipRebateInfo>("Admin_VipRebateReportListbyCust_Get_V3", props).ToList();

            searchData.VipRebateInfoList = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return searchData;
        }
        public VipRebateInfoData GetVipRebateInfoListExport(string selectSite, int currency, long startDate, long endDate, string User, int CategoryId, int BaseCurrencyId, int RebateCategoriesId)
        {
            var SearchData = new VipRebateInfoData();
            var props = new DynamicParameters();
            props.Add("SelectSite", selectSite);
            props.Add("Currency", currency);
            props.Add("UserName", string.IsNullOrWhiteSpace(User) ? null : User);
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("CategoryId", CategoryId);
            props.Add("BaseCurrencyId", BaseCurrencyId);
            props.Add("CategoryType", RebateCategoriesId);
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<VipRebateInfo>("Admin_VipRebateReportListV4Export_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.VipRebateInfoList = result;
            SearchData.PageNumber = 0;
            SearchData.TotalPages = 0;
            SearchData.RowCount = 0;
            return SearchData;
        }
        public IEnumerable<VipTPCategory> GetTPCategoryList()
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Query<VipTPCategory>("Admin_TPCategory_Get", commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<TpBetRecordListOneWeek> GetCustTpBetRecordList(int custId, DateTime startDate, DateTime endDate)
        {
            var result = _reportDbClient.Query<TpBetRecordListOneWeek>("Admin_TpBetRecordList_Get_V4", new DynamicParameters(new { CustId = custId, StartDate = startDate, EndDate = endDate }));
            return result;
        }
    }
}
