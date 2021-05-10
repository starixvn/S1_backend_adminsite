using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var depositList = conn.Query<FinancialLog>("Admin_FinancialLogList_Get", p, commandType: CommandType.StoredProcedure);
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
            var depositList = conn.Query<BonusLog>("Admin_BonusLogList_Get", p, commandType: CommandType.StoredProcedure);
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
        public IEnumerable<RebateLog> GetRebateLog(IEnumerable<int> SelectSite, string userName, int rebateType, long startDate, long endDate, int pageSize, int currencyId, int baseCurrencyId, decimal totalAmountShow, int rebateCategoriesId, ref int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters(new { SelectSite = JsonConvert.SerializeObject(SelectSite), UserName = userName, RebateType = rebateType
                , StartDate = startDate, EndDate = endDate, PageSize = pageSize, CategoryType= rebateCategoriesId, CurrencyId= currencyId,BaseCurrencyId= baseCurrencyId,TotalAmountShow= totalAmountShow});
            p.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var rebateLogList = conn.Query<RebateLog>("Admin_RebateLogListV2_Get", p, commandType: CommandType.StoredProcedure);
            pageNumber = p.Get<int?>("PageNumber") ?? 0;
            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;
            return rebateLogList;
        }
        public IEnumerable<RebateLog> GetRebateLogExport(string SelectSite, string userName, int rebateType, long startDate, long endDate, int currencyId, int baseCurrencyId, decimal totalAmountShow, int rebateCategoriesId)
        {
            var p = new DynamicParameters(new { SelectSite, UserName = userName, RebateType = rebateType, StartDate = startDate, EndDate = endDate, CategoryType = rebateCategoriesId, CurrencyId = currencyId, BaseCurrencyId = baseCurrencyId, TotalAmountShow = totalAmountShow });
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var rebateLogList = conn.Query<RebateLog>("Admin_RebateLogListExportV2_Get", p, commandType: CommandType.StoredProcedure);
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
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<PromotionCategory>("Common_PromotionCategory_Get", commandType: CommandType.StoredProcedure);
            return result;
        }
        public IEnumerable<TpBetRecordListOneWeek> GetCustTpBetRecordListOneWeek(int custId)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<TpBetRecordListOneWeek>("Admin_TpBetRecord_byCustIDOneWeek_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public IEnumerable<TpBetRecordListOneDay> GetCustTpBetRecordListOneDay(int custId, string selDate)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<TpBetRecordListOneDay>("Admin_TpBetRecord_byOneDay_Get", new { CustId = custId, SelDate = selDate }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public IEnumerable<TpBetRecordbyCategoryId> GetCustTpBetRecordListbyCategoryId(int custId, string selDate, int categoryId)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<TpBetRecordbyCategoryId>("Admin_TpBetRecord_byCategoryId_Get", new  { CustId = custId, SelDate = selDate, CategoryId = categoryId }, commandType: CommandType.StoredProcedure);
            return result;
        }
        public IEnumerable<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailListbyGameType(int custId, string selDate, int gameTypeId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {

            IEnumerable<TpBetRecordDetailbyGameType> result = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Admin_TpBetRecord_byGameTypeList_Get";
                DynamicParameters props = new DynamicParameters(new { CustId = custId, SelDate = selDate, TypeId = gameTypeId });
                props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@PageSize", PageSize);

                result = conn.Query<TpBetRecordDetailbyGameType>(SpName, props, commandType: CommandType.StoredProcedure);
                PageNumber = props.Get<int?>("@PageNumber") ?? 0;
                TotalPages = props.Get<int?>("@TotalPages") ?? 0;
                RowCount = props.Get<int?>("@RowCount") ?? 0;
            }

            return result;
        }

        public VipRebateInfoData GetVipRebateInfoList(IEnumerable<int> selectSite, int currency, long startDate, long endDate, string User, int pageSize, int pageNumber,int CategoryId)
        {
            VipRebateInfoData SearchData = new VipRebateInfoData();
            DynamicParameters props = new DynamicParameters();
            props.Add("SelectSite", JsonConvert.SerializeObject(selectSite));
            props.Add("Currency", currency);
            props.Add("UserName", string.IsNullOrWhiteSpace(User) ? null : User);
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("PageSize", pageSize);
            props.Add("CategoryId", CategoryId);
            props.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<VipRebateInfo>("Admin_VipRebateReportListV2_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.VipRebateInfoList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return SearchData;
        }
        public VipRebateInfoData GetVipRebateInfoListByCustId(int custId, long? startDate, long? endDate, int pageSize, int pageNumber)
        {
            VipRebateInfoData SearchData = new VipRebateInfoData();
            DynamicParameters props = new DynamicParameters();
            props.Add("CustId", custId);
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("PageSize", pageSize);
            props.Add("PageNumber", pageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<VipRebateInfo>("Admin_VipRebateReportListbyCust_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.VipRebateInfoList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;
            return SearchData;
        }
        public VipRebateInfoData GetVipRebateInfoListExport(string selectSite, int currency, long startDate, long endDate, string User, int CategoryId)
        {
            VipRebateInfoData SearchData = new VipRebateInfoData();
            DynamicParameters props = new DynamicParameters();
            props.Add("SelectSite", selectSite);
            props.Add("Currency", currency);
            props.Add("UserName", string.IsNullOrWhiteSpace(User) ? null : User);
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("CategoryId", CategoryId);
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var result = conn.Query<VipRebateInfo>("Admin_VipRebateReportListV2Export_Get", props, commandType: CommandType.StoredProcedure).ToList();

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
    }
}
