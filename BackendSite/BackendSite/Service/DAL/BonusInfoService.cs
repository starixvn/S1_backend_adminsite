using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

namespace BackendSite.Service.DAL
{
    public class BonusInfoService
    {
        private readonly IConfiguration _configuration;
        private readonly WalletService _walletService;
        private readonly WithdrawalLimitService _withdrawalLimitService;
        private readonly CustInfoService _custInfoService;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public BonusInfoService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _walletService = provider.GetRequiredService<WalletService>();
            _withdrawalLimitService = provider.GetRequiredService<WithdrawalLimitService>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
        }

        public IEnumerable<BonusInfo> GetBonusInfoReport(string siteIds,
            string bonusCode,
            string transId,
            int currencyId,
            string userName,
            int promotionType,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId,
            int rebateCategoriesId,
            string statuses,
            int startIndex,
            int pageSize,
            out int totalPages, out int rowCount, out List<string> currencyAmount)
        {
            var tempCurrencyTotalAmount = new List<string>();

            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);
            props.Add("_BonusCode", bonusCode);
            props.Add("_TransId", transId);
            props.Add("_CurrencyId", currencyId);
            props.Add("_UserName", userName);
            props.Add("_PromotionType", promotionType);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_CategoryType", rebateCategoriesId);
            props.Add("_Statuses", statuses);

            #region GetCurrencyTotalAmount

            var currencyTotalAmount = _reportDbClient.Query<TotalAmount>("Admin_BonusInfoTotalAmount_Get_V5", props).ToList();
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

            var result = _reportDbClient.Query<BonusInfo>("Admin_BonusInfo_Get_V8", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public IEnumerable<BonusInfo> GetBonusInfoExportReport(string siteIds,
            string bonusCode,
            string transId,
            int currencyId,
            string userName,
            int promotionType,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId,
            int rebateCategoriesId,
            string statuses)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);
            props.Add("_BonusCode", bonusCode);
            props.Add("_TransId", transId);
            props.Add("_CurrencyId", currencyId);
            props.Add("_UserName", userName);
            props.Add("_PromotionType", promotionType);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_CategoryType", rebateCategoriesId);
            props.Add("_Statuses", statuses);

            return _reportDbClient.Query<BonusInfo>("Admin_BonusInfoExport_Get_V8", props);
        }

        public IEnumerable<PendingBonus> GetPendingBonusList(string siteIds)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);

            return _mainDbClient.Query<PendingBonus>("Admin_PendingBonusList_Get_V5", props);
        }

        public CancelBonus GetTransferBonusCancel(int bonusId)
        {
            var props = new DynamicParameters();
            props.Add("_BonusId", bonusId);
            return _reportDbClient.QueryFirst<CancelBonus>("Admin_BonusCancel_Get", props);
        }

        public IEnumerable<decimal> GetBonusWinLoss(int bonusId)
        {
            var props = new DynamicParameters();
            props.Add("BonusId", bonusId);
            return _reportDbClient.Query<decimal>("Service_BonusWinLoss_Get", props);
        }

        public async Task<(int Result, int CustId)> CancelBonusByBonusId(bool isReturnAmount, int bonusId, string statusList, string @operator)
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
            var result = _mainDbClient.Execute("Common_CancelBonusByBonusId_SetV3", p, conn, trans);

            var siteId = p.Get<int?>("_SiteId") ?? 0;
            var custId = p.Get<int?>("_CustId") ?? 0;
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
                    result = _mainDbClient.Execute("Service_CancelBonusAmount_Set", cp, conn, trans);
                }
            }

            var custSimpleInfo = await _custInfoService.GetCustId(custId, null, null);
            if (!await _custInfoService.InsertLogAsync(custSimpleInfo.SiteId, custId, custSimpleInfo.CurrencyId, new { BonusId = bonusId, Status = "Running" },
               new { BonusId = bonusId, Status = "Cancel" }, AdminAdjustmentLogMenuEnum.CancelBonusInCustInfo, ActionType.Update, @operator, conn, trans))
            {
                trans.Rollback();
                return (0, custId);
            }

            if (result > 0)
                trans.Commit();

            return (result, custId);
        }

        public IEnumerable<BonusCodeInfo> GetBonusCodeList(IEnumerable<int> bonusIdList)
        {
            return _reportDbClient.Query<BonusCodeInfo>("Admin_BonusCode_Get", new { BonusIdList = JsonConvert.SerializeObject(bonusIdList) });
        }

        public CustBonusSum GetCustBonusSum(int siteId, int custId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CustId", custId);
            var result = _reportDbClient.QueryFirst<CustBonusSum>("Admin_CustBonusSum_Get", props);
            return result;
        }

        public CustRebateSum GetCustRebateSum(int siteId, int custId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CustId", custId);
            var custRebateCategory = _reportDbClient.Query<CustRebateCategory>("Admin_CustRebateSum_Get", props);

            decimal custRebateTotalSum = 0;
            if (custRebateCategory != null)
            {
                custRebateTotalSum = custRebateCategory.Sum(s => s.RebateAmountSum);
            }

            var result = new CustRebateSum()
            {
                CustRebateCategory = custRebateCategory,
                CustRebateTotalSum = custRebateTotalSum
            };

            return result;
        }

        public CustRewardSum GetCustRewardSum(int siteId, int custId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CustId", custId);
            var custRewardType = _reportDbClient.Query<CustRewardType>("Admin_CustRewardSum_Get", props);

            decimal custRebateTotalSum = 0;
            if (custRewardType != null)
            {
                custRebateTotalSum = custRewardType.Sum(s => s.RewardAmountSum);
            }

            var result = new CustRewardSum()
            {
                CustRewardType = custRewardType,
                CustRewardTotalSum = custRebateTotalSum
            };

            return result;
        }
    }
}
