using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class BonusInfoService
    {
        private readonly WalletService _walletService;
        private readonly WithdrawalLimitService _withdrawalLimitService;
        private readonly CustInfoService _custInfoService;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public BonusInfoService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _walletService = provider.GetRequiredService<WalletService>();
            _withdrawalLimitService = provider.GetRequiredService<WithdrawalLimitService>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
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

        public async Task<(int Result, int CustId)> CancelBonusByBonusId(int bonusId, string statusList, string @operator)
        {
            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();
            var p = new DynamicParameters(new { _BonusId = bonusId, _StatusList = statusList });
            p.Add("_SiteId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_UserName", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("_LockedAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_BonusAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_Bonus", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            var result = _mainDbClient.Execute("Common_CancelBonusByBonusId_SetV3", trans, p);

            var siteId = p.Get<int?>("_SiteId") ?? 0;
            var custId = p.Get<int?>("_CustId") ?? 0;
            var userName = p.Get<string>("_UserName");
            var lockedAmount = p.Get<decimal?>("_LockedAmount") ?? 0;
            var bonusAmount = p.Get<decimal?>("_BonusAmount") ?? 0;
            var bonus = p.Get<decimal?>("_Bonus") ?? 0;

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
                result = _walletService.SetBalance(custId, siteId, userName, backAmount, 7, $"Cancel Bonus({bonus.ToString("#,##0.##")}), Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out var afterAmount, out var is1stDepoist, trans);

                if (result > 0)
                    result = _withdrawalLimitService.SetWithdrawalLimit(custId, userName, siteId, backAmount, (int)WithdrawalLimitAdjustmentReasonEnum.None, $"Cancel Bonus, Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), (int)WithdrawalLimitLogCreateFromEnum.Bonus, @operator, out var afterWithdrawalLimit, trans);
            }

            //回收bonus紀錄
            var cancelBonusAmount = bonus - backAmount;
            if (cancelBonusAmount > 0 && result > 0)
            {
                var cp = new DynamicParameters(new { _BonusId = bonusId, _CancelBonus = cancelBonusAmount });
                result = _mainDbClient.Execute("Service_CancelBonusAmount_Set", trans, cp);
            }

            var custSimpleInfo = await _custInfoService.GetCustId(custId, null, null);
            if (!await _custInfoService.InsertLogAsync(custSimpleInfo.SiteId, custId, custSimpleInfo.CurrencyId, new { BonusId = bonusId, Status = "Running" },
               new { BonusId = bonusId, Status = "Cancel" }, AdminAdjustmentLogMenuEnum.CancelBonusInCustInfo, ActionType.Update, @operator, trans))
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return (0, custId);
            }

            if (result > 0)
            {
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
            }
            else
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
            }

            return (result, custId);
        }
        
        public List<MpaBonusInfo> GetMpaBonusInfoList(
            string multiSiteRequests,
            string bonusStatuses,
            string bonusTypes,
            string bonusCode,
            DateTime startDate,
            DateTime endDate,
            string claimStatuses,
            string timeZone)
        {
            var props = new DynamicParameters();
            props.Add("_MultiSiteRequest", multiSiteRequests);
            props.Add("_Statuses", bonusStatuses);
            props.Add("_BonusTypes", bonusTypes);
            props.Add("_BonusCode", bonusCode);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_ClaimStatuses", claimStatuses);
            props.Add("_TimeZone", timeZone);

            var result = _reportDbClient.Query<MpaBonusInfo>("Admin_BonusSummaryInfo_Get_V3", props);

            return result.ToList();
        }

        public List<MpaBonusGGR> GetMpaBonusGGRList(
            string multiSiteRequests,
            DateTime startDate,
            DateTime endDate,
            string timeZone)
        {
            var props = new DynamicParameters();
            props.Add("_MultiSiteRequest", multiSiteRequests);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_TimeZone", timeZone);

            var result = _reportDbClient.Query<MpaBonusGGR>("Admin_BonusSummaryGGR_Get_V2", props);

            return result.ToList();
        }

        public List<MpaBonusDetail> GetMpaBonusDetailList(
            string multiSiteRequest,
            string bonusCode,
            DateTime startDate,
            DateTime endDate,
            string claimStatuses,
            int baseCurrencyId,
            string timeZone)
        {
            var props = new DynamicParameters();
            props.Add("_MultiSiteRequest", multiSiteRequest);
            props.Add("_BonusCode", bonusCode);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_ClaimStatuses", claimStatuses);
            props.Add("_BaseCurrencyId", baseCurrencyId);
            props.Add("_TimeZone", timeZone);

            var result = _reportDbClient.Query<MpaBonusDetail>("Admin_BonusSummaryDetail_Get_V2", props);
            return result.ToList();
        }

        public IEnumerable<MpaMarketingBonus> GetMpaMarketingBonus(
            string multiSiteRequest,
            DateTime startDate,
            DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("_MultiSiteRequest", multiSiteRequest);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);

            var result = _reportDbClient.Query<MpaMarketingBonus>("Admin_MarketingBonusSummary_Get_V2", props);

            return result;
        }
        
        public CustBonusSum GetCustBonusSum(int siteId,int custId)
        {
            var props = new DynamicParameters();
            props.Add("_CustId", custId);
            props.Add("_RewardPoint", dbType: DbType.Decimal, direction: ParameterDirection.Output);

            var data =
                _reportDbClient.Query<CustBonusTypeBonusAmount>("Admin_CustBonusSum_Get_V2", props);

            var result = new CustBonusSum
            {
                FirstDeposit = data.Where(x => x.PromotionType == PromotionTypeEnum.FstDeposit)
                    .Sum(x => x.BonusAmount),
                ReDeposit = data.Where(x => x.PromotionType == PromotionTypeEnum.ReDeposit)
                    .Sum(x => x.BonusAmount),
                FreeBet = data.Where(x => x.PromotionType == PromotionTypeEnum.FreeBet)
                    .Sum(x => x.BonusAmount),
                CashBack = data.Where(x => x.PromotionType == PromotionTypeEnum.Cashback)
                    .Sum(x => x.BonusAmount),
                ManualBonus = data.Where(x => x.PromotionType == PromotionTypeEnum.SpecialBonus)
                    .Sum(x => x.BonusAmount),
                RetentionBonus = data.Where(x => x.PromotionType == PromotionTypeEnum.RetentionFreeBet)
                    .Sum(x => x.BonusAmount),
                DeviceBonus = data.Where(x => x.PromotionType == PromotionTypeEnum.DeviceBonus)
                    .Sum(x => x.BonusAmount),
                CancelBonusAmount = data.Sum(x => x.CancelBonus),
                RewardPoint = props.Get<decimal?>("_RewardPoint") ?? 0
            };

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

        public IEnumerable<BonusCodeInfo> GetBonusCodeList(IEnumerable<int> bonusIdList)
        {
            return _reportDbClient.Query<BonusCodeInfo>("Admin_BonusCode_Get", new { BonusIdList = JsonConvert.SerializeObject(bonusIdList) });
        }
        
        
        public List<MpaMarketingBonusDetail> GetMpaMarketingBonusDetail(
            int siteId,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyIds", currencyIds);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_BaseCurrencyId", baseCurrencyId);

            var result = _reportDbClient.Query<MpaMarketingBonusDetail>("Admin_MarketingBonusSummaryDetail_Get", props);

            return result.ToList();
        }
        
        public int GetEggTicket(int siteId,int custId,int currencyId)
        {
            var p = new DynamicParameters(new
            {
                _SiteId = siteId,
                _CustId = custId,
                _CurrencyId = currencyId
            });
            var eggTicket = _reportDbClient.QueryFirst<int>("Service_CustEggTicket_Get", p);
            
            return eggTicket;
        }
    }
}
