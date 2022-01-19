using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class BonusInfoService
    {
        private readonly IConfiguration _configuration;
        private readonly WalletService _walletService;
        private readonly WithdrawalLimitService _withdrawalLimitService;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public BonusInfoService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _walletService = provider.GetRequiredService<WalletService>();
            _withdrawalLimitService = provider.GetRequiredService<WithdrawalLimitService>();
        }

        public IEnumerable<BonusInfo> GetBonusInfoReport(IEnumerable<int> selectSite, string bonusCode, string transId, int currencyId, string userName, int promotionType, IEnumerable<int> siteList, DateTime startDate, DateTime endDate, int pageSize, int baseCurrencyId, int rebateCategoriesId, int startIndex, out int totalPages, out int rowCount, out List<string> currencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();

            var props = new DynamicParameters();
            props.Add("_SelectSite", JsonConvert.SerializeObject(selectSite));
            props.Add("_BonusCode", bonusCode);
            props.Add("_SiteIds", JsonConvert.SerializeObject(siteList));
            props.Add("_PromotionType", promotionType);
            props.Add("_Currency", currencyId);
            props.Add("_UserName", userName);
            props.Add("_TransID", transId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_CategoryType", rebateCategoriesId);
            props.Add("_BaseCurrencyId", baseCurrencyId);

            #region GetCurrencyTotalAmount

            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_BonusInfoTotalAmount_Get_V4", props).ToList();
            if (currencyTotalAmount.Count > 0)
            {
                currencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add($"{x.Currency}:{Math.Round(x.Amount, 2)}");
                });
            }
            currencyAmount = tempCurrencyTotalAmount;

            #endregion

            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<BonusInfo>("Admin_BonusInfo_Get_V7", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<BonusInfo> GetBonusInfoExportReport(string selectSite, string bonusCode, string transId, int currencyId, string userName, int promotionType, IEnumerable<int> siteList, DateTime startDate, DateTime endDate, int baseCurrencyId, int rebateCategoriesId)
        {
            var props = new DynamicParameters();
            props.Add("_SelectSite", selectSite);
            props.Add("_BonusCode", bonusCode);
            props.Add("_SiteIds", JsonConvert.SerializeObject(siteList));
            props.Add("_PromotionType", promotionType);
            props.Add("_Currency", currencyId);
            props.Add("_UserName", userName);
            props.Add("_TransID", transId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_CategoryType", rebateCategoriesId);

            return _reportDbClient.Query<BonusInfo>("Admin_BonusInfoExport_Get_V7", props);

        }

        public IEnumerable<PendingBonus> GetPendingBonusList(int startIndex, int pageSize, out int totalPages, out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_PageSize", pageSize);
            props.Add("_StartIndex", startIndex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _mainDbClient.Query<PendingBonus>("Admin_PendingBonusList_Get_V3", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public CancelBonus GetTransferBonusCancel(int bonusId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<CancelBonus>("Admin_BonusCancel_Get", new { _BonusId = bonusId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<decimal> GetBonusWinLoss(int bonusId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<decimal>("Service_BonusWinLoss_Get", new { BonusId = bonusId }, commandType: CommandType.StoredProcedure);
        }

        public int CancelBonusByBonusId(bool isReturnAmount, int bonusId, string statusList, out int custId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var p = new DynamicParameters(new { _BonusId = bonusId, _StatusList = statusList });
            p.Add("_SiteId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_UserName", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("_LockedAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_BonusAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_Bonus", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            var result = conn.Execute("Common_CancelBonusByBonusId_SetV3", p, trans, commandType: CommandType.StoredProcedure);

            var siteId = p.Get<int?>("_SiteId") ?? 0;
            custId = p.Get<int?>("_CustId") ?? 0;
            var userName = p.Get<string>("_UserName");
            var lockedAmount = p.Get<decimal?>("_LockedAmount") ?? 0;
            var bonusAmount = p.Get<decimal?>("_BonusAmount") ?? 0;
            var bonus = p.Get<decimal?>("_Bonus") ?? 0;

            if (isReturnAmount)
            {
                decimal backAmount = 0;
                if (bonus >= lockedAmount + bonusAmount)
                {
                    backAmount = lockedAmount;
                }
                else
                {
                    backAmount = bonus - bonusAmount;
                }

                //避免輸光 負負得正
                if (backAmount < 0)
                {
                    backAmount = 0;
                }

                if (backAmount > 0)
                {
                    result = _walletService.SetBalance(custId, siteId, userName, backAmount, 7, $"Cancel Bonus({bonus.ToString("#,##0.##")}), Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out var afterAmount, out var is1stDepoist, conn, trans);

                    if (result > 0)
                        result = _withdrawalLimitService.SetWithdrawalLimit(custId, userName, siteId, backAmount, $"Cancel Bonus, Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out var afterWithdrawalLimit, conn, trans);
                }

                //回收bonus紀錄
                var cancelBonusAmount = bonus - backAmount;
                if (cancelBonusAmount > 0 && result > 0)
                {
                    var cp = new DynamicParameters(new { _BonusId = bonusId, _CancelBonus = cancelBonusAmount });
                    result = conn.Execute("Service_CancelBonusAmount_Set", cp, trans, commandType: CommandType.StoredProcedure);
                }
            }

            if (result > 0)
                trans.Commit();

            return result;
        }
    }
}
