using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using BackendSite.Service.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class WalletService
    {
        private readonly IConfiguration configuration;

        public WalletService(IConfiguration configuration)
        {
            this.configuration = configuration;
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
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
        public IEnumerable<FinancialLog> GetFinancialLog(int CustId, long? startDate, long? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { CustId = CustId, StartDate = startDate, EndDate = endDate, RefTransId = refTransId, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<FinancialLog>("Admin_FinancialLogListV2_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return depositList;
        }
        public IEnumerable<BonusLog> GetBonusLog(int CustId, int promoCategoryId, long? startDate, long? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { CustId = CustId, PromoCategoryId = promoCategoryId, StartDate = startDate, EndDate = endDate, RefTransId = refTransId, PageSize = pageSize });
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var depositList = conn.Query<BonusLog>("Admin_BonusLogListV2_Get", p, commandType: CommandType.StoredProcedure);
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

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var rebateLogList = conn.Query<RebateLog>("Admin_RebateLogListByCust_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return rebateLogList;
        }
        public IEnumerable<RebateLog> GetRebateLog(IEnumerable<int> SelectSite, string userName, int rebateType, long startDate, long endDate, int pageSize, int currencyId, int baseCurrencyId, int rebateCategoriesId, ref int pageNumber, out int totalPages, out int rowCount, ref List<string> CurrencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var rebateLogList = conn.Query<RebateLog>("Admin_RebateLogListExportV3_Get", p, commandType: CommandType.StoredProcedure);
            return rebateLogList;
        }
        public IEnumerable<RebateInfo> GetRebateInfoByCust(int CustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var rebateInfoList = conn.Query<RebateInfo>("Admin_RebateInfoByCust_Get", new { CustId }, commandType: CommandType.StoredProcedure);
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

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var rebateInfoList = conn.Query<RebateInfo>("Admin_RebateInfoList_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return rebateInfoList;
        }
        public IEnumerable<RebateInfo> GetRebateInfoExport(string SelectSite, string userName, int rebateType, long startDate, long endDate)
        {
            var p = new DynamicParameters(new { SelectSite, UserName = userName, RebateType = rebateType, StartDate = startDate, EndDate = endDate });
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var rebateInfoList = conn.Query<RebateInfo>("Admin_RebateInfoListExport_Get", p, commandType: CommandType.StoredProcedure);
            return rebateInfoList;
        }

        public IEnumerable<PromotionCategory> GetPromotionCategory()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<PromotionCategory>("Common_PromotionCategory_Get", commandType: CommandType.StoredProcedure);
            return result;
        }
        public IEnumerable<TpBetRecordListOneWeek> GetCustTpBetRecordListOneWeek(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<TpBetRecordListOneWeek>("Admin_TpBetRecord_byCustIDOneWeek_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public IEnumerable<TpBetRecordListOneDay> GetCustTpBetRecordListOneDay(int custId, string selDate, long startDate, long endDate)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<TpBetRecordListOneDay>("Admin_TpBetRecord_byOneDayV4_Get", new { CustId = custId, SelDate = selDate, StartDate = startDate, EndDate = endDate }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public IEnumerable<TpBetRecordbyCategoryId> GetCustTpBetRecordListbyCategoryId(int custId, string selDate, int categoryId, long startDate, long endDate)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<TpBetRecordbyCategoryId>("Admin_TpBetRecord_byCategoryIdV4_Get", new  { CustId = custId, SelDate = selDate, CategoryId = categoryId, StartDate = startDate, EndDate = endDate }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public IEnumerable<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(int custId, string selDate, int gameTypeId, long startDate, long endDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {

            IEnumerable<TpBetRecordDetailbyGameType> result = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var SpName = "Admin_TpBetRecord_byGameTypeListV4_Get";
            var props = new DynamicParameters(new { CustId = custId, SelDate = selDate, TypeId = gameTypeId, StartDate = startDate, EndDate = endDate });
            props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@PageSize", PageSize);

            result = conn.Query<TpBetRecordDetailbyGameType>(SpName, props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("@PageNumber") ?? 0;
            TotalPages = props.Get<int?>("@TotalPages") ?? 0;
            RowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public VipRebateInfoData GetVipRebateInfoList(IEnumerable<int> selectSite, int currency, long startDate, long endDate, string User, int pageSize, int pageNumber,int CategoryId, int BaseCurrencyId,ref List<string> CurrencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();
            var SearchData = new VipRebateInfoData();
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
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

            var result = conn.Query<VipRebateInfo>("Admin_VipRebateReportListV6_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.VipRebateInfoList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return SearchData;
        }
        public VipRebateInfoData GetVipRebateInfoListByCustId(int custId, long? startDate, long? endDate, int pageSize, int pageNumber)
        {
            var SearchData = new VipRebateInfoData();
            var tempCurrencyTotalAmount = new List<string>();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_VipRebateTotalAmountbyCustV2_Get", props, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            SearchData.CurrencyAmount = tempCurrencyTotalAmount;
            #endregion


            props.Add("PageSize", pageSize);
            props.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var result = conn.Query<VipRebateInfo>("Admin_VipRebateReportListbyCustV2_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.VipRebateInfoList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return SearchData;
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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<VipRebateInfo>("Admin_VipRebateReportListV4Export_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.VipRebateInfoList = result;
            SearchData.PageNumber = 0;
            SearchData.TotalPages = 0;
            SearchData.RowCount = 0;
            return SearchData;
        }
        public IEnumerable<VipTPCategory> GetTPCategoryList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<VipTPCategory>("Admin_TPCategory_Get", commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<TpBetRecordListOneWeek> GetCustTpBetRecordList(int custId, long startDate, long endDate)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<TpBetRecordListOneWeek>("Admin_TpBetRecordListV3_Get", new { CustId = custId, StartDate = startDate, EndDate = endDate }, commandType: CommandType.StoredProcedure);
            return result;
        }
    }
}
