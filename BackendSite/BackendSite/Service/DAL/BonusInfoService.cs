using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class BonusInfoService
    {
        private readonly IConfiguration configuration;
        private readonly WalletService walletService;
        private readonly WithdrawalLimitService withdrawalLimitService;

        public BonusInfoService(IConfiguration configuration, WalletService walletService, WithdrawalLimitService withdrawalLimitService)
        {
            this.configuration = configuration;
            this.walletService = walletService;
            this.withdrawalLimitService = withdrawalLimitService;
        }

        public IEnumerable<BonusInfo> GetBonusInfoReport(IEnumerable<int> SelectSite, string BonusCode, string TransId, int CurrencyId, string UserName, int PromotionType, IEnumerable<int> SiteList, long StartDate, long EndDate, int PageSize, int BaseCurrencyId, int RebateCategoriesId, ref int PageNumber, ref int TotalPages, ref int RowCount, ref List<string> CurrencyAmount)
        {
            IEnumerable<BonusInfo> result = null;
            var tempCurrencyTotalAmount = new List<string>();

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("@SelectSite", JsonConvert.SerializeObject(SelectSite));
            props.Add("@BonusCode", BonusCode);
            props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));
            props.Add("@PromotionType", PromotionType);
            props.Add("@Currency", CurrencyId);
            props.Add("@UserName", UserName);
            props.Add("@TransID", TransId);
            props.Add("@StartDate", StartDate);
            props.Add("@EndDate", EndDate);
            props.Add("@CategoryType", RebateCategoriesId);
            props.Add("@BaseCurrencyId", BaseCurrencyId);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_BonusInfoTotalAmountV2_Get", props, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            CurrencyAmount = tempCurrencyTotalAmount;
            #endregion


            props.Add("@PageSize", PageSize);
            props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result = conn.Query<BonusInfo>("Admin_BonusInfoV4_Get", props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("@PageNumber") ?? 0;
            TotalPages = props.Get<int?>("@TotalPages") ?? 0;
            RowCount = props.Get<int?>("@RowCount") ?? 0;


            return result;
        }

        public IEnumerable<BonusInfo> GetBonusInfoExportReport(string SelectSite, string BonusCode, string TransId, int CurrencyId, string UserName, int PromotionType, IEnumerable<int> SiteList, long StartDate, long EndDate, int BaseCurrencyId, int RebateCategoriesId)
        {
            IEnumerable<BonusInfo> result = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var SpName = "Admin_BonusInfoExportV4_Get";
            var props = new DynamicParameters();
            props.Add("@SelectSite", SelectSite);
            props.Add("@BonusCode", BonusCode);
            props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));
            props.Add("@PromotionType", PromotionType);
            props.Add("@Currency", CurrencyId);
            props.Add("@UserName", UserName);
            props.Add("@TransID", TransId);
            props.Add("@StartDate", StartDate);
            props.Add("@EndDate", EndDate);
            props.Add("@BaseCurrencyId", BaseCurrencyId);
            props.Add("@CategoryType", RebateCategoriesId);

            result = conn.Query<BonusInfo>(SpName, props, commandType: CommandType.StoredProcedure);

            return result;
        }

        public IEnumerable<PendingBonus> GetPendingBonusList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var SpName = "Admin_PendingBonusList_Get";
            return conn.Query<PendingBonus>(SpName, commandType: CommandType.StoredProcedure);
        }
        
        public CancelBonus GetTransferBonusCancel(int bonusId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<CancelBonus>("Admin_TransferBonusCancel_Get",new { _BonusId = bonusId },commandType: CommandType.StoredProcedure);
        }
        
        public IEnumerable<decimal> GetBonusWinLoss(int bonusId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<decimal>("Service_BonusWinLoss_Get", new { BonusId = bonusId },commandType: CommandType.StoredProcedure);
        }
        
        public int CancelBonusByBonusId(bool isReturnAmount, int bonusId, string statusList, out int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var p = new DynamicParameters(new { _BonusId = bonusId, _StatusList = statusList });
            p.Add("_SiteId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_UserName", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("_LockedAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_BonusAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("_Bonus", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            var result = conn.Execute("Common_CancelBonusByBonusId_SetV3", p, transaction: trans, commandType: CommandType.StoredProcedure);

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

                if (backAmount > 0)
                {
                    result = walletService.SetBalance(custId, siteId, userName, backAmount, 7, $"Cancel Bonus({bonus.ToString("#,##0.##")}), Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out var afterAmount, out var is1stDepoist, conn, trans);

                    if (result > 0)
                        result = withdrawalLimitService.SetWithdrawalLimit(custId, userName, siteId, backAmount, $"Cancel Bonus, Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out var afterWithdrawalLimit, conn, trans);
                }
            }

            if (result > 0)
                trans.Commit();

            return result;
        }
    }
}
