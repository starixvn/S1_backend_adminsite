using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public IEnumerable<BonusInfo> GetBonusInfoReport(IEnumerable<int> SelectSite, string BonusCode, string TransId, int CurrencyId, string UserName, int PromotionType, IEnumerable<int> SiteList, long StartDate, long EndDate, int PageSize, int BaseCurrencyId, decimal TotalAmountShow, int RebateCategoriesId ,ref int PageNumber, ref int TotalPages, ref int RowCount,ref List<string> CurrencyAmount)
        {
            IEnumerable<BonusInfo> result = null;
            List<string> tempCurrencyTotalAmount = new List<string>();

            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("@SelectSite", JsonConvert.SerializeObject(SelectSite));
            props.Add("@BonusCode", BonusCode);
            props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));
            props.Add("@PromotionType", PromotionType);
            props.Add("@Currency", CurrencyId);
            props.Add("@UserName", UserName);
            props.Add("@TransID", TransId);
            props.Add("@StartDate", StartDate);
            props.Add("@EndDate", EndDate);
            props.Add("@TotalAmountShow", TotalAmountShow);
            props.Add("@CategoryType", RebateCategoriesId);
            props.Add("@BaseCurrencyId", BaseCurrencyId);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_BonusInfoTotalAmount_Get", props, commandType: CommandType.StoredProcedure).ToList();
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

            result = conn.Query<BonusInfo>("Admin_BonusInfoV3_Get", props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("@PageNumber") ?? 0;
            TotalPages = props.Get<int?>("@TotalPages") ?? 0;
            RowCount = props.Get<int?>("@RowCount") ?? 0;


            return result;
        }

        public IEnumerable<BonusInfo> GetBonusInfoExportReport(string SelectSite, string BonusCode, string TransId, int CurrencyId, string UserName, int PromotionType, IEnumerable<int> SiteList, long StartDate, long EndDate, int BaseCurrencyId, decimal TotalAmountShow, int RebateCategoriesId)
        {
            IEnumerable<BonusInfo> result = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_BonusInfoExportV3_Get";
                DynamicParameters props = new DynamicParameters();
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
                props.Add("@TotalAmountShow", TotalAmountShow);
                props.Add("@CategoryType", RebateCategoriesId);

                result = conn.Query<BonusInfo>(SpName, props, commandType: CommandType.StoredProcedure);
            }

            return result;
        }

        public IEnumerable<PendingBonus> GetPendingBonusList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_PendingBonusList_Get";
                return conn.Query<PendingBonus>(SpName, commandType: CommandType.StoredProcedure);
            }
        }
        public int CancelBonusByBonusId(int bonusId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();
            var p = new DynamicParameters(new { BonusId = bonusId });
            p.Add("SiteId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("CustId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("UserName", dbType: DbType.String, direction: ParameterDirection.Output);
            p.Add("LockedAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("BonusAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            p.Add("Bonus", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            int result = conn.Execute("Common_CancelBonusByBonusId_SetV2", p, transaction: trans, commandType: CommandType.StoredProcedure);

            int siteId = p.Get<int?>("SiteId") ?? 0;
            int custId = p.Get<int?>("CustId") ?? 0;
            string userName = p.Get<string>("UserName");
            decimal lockedAmount = p.Get<decimal?>("LockedAmount") ?? 0;
            decimal bonusAmount = p.Get<decimal?>("BonusAmount") ?? 0;
            decimal bonus = p.Get<decimal?>("Bonus") ?? 0;

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
                result = walletService.SetBalance(custId, siteId, userName, backAmount, 7, $"Cancel Bonus({bonus.ToString("#,##0.##")}), Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out decimal afterAmount, out byte is1stDepoist, conn, trans);

                if (result > 0)
                    result = withdrawalLimitService.SetWithdrawalLimit(custId, userName, siteId, backAmount, $"Cancel Bonus, Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out decimal afterWithdrawalLimit, conn, trans);
            }

            if (result > 0)
                trans.Commit();

            return result;
        }
    }
}
