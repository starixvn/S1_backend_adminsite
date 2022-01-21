using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using BackendSite.Service.Model.enums;
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

        public IEnumerable<BonusInfo> GetBonusInfoReport(IEnumerable<int> SelectSite, string BonusCode, string TransId, int CurrencyId, string UserName, int PromotionType, IEnumerable<int> SiteList, DateTime StartDate, DateTime EndDate, int PageSize, int BaseCurrencyId, int RebateCategoriesId, int Startindex, out int TotalPages, out int RowCount, out List<string> CurrencyAmount)
        {
            IEnumerable<BonusInfo> result = null;
            var tempCurrencyTotalAmount = new List<string>();

            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("_SelectSite", JsonConvert.SerializeObject(SelectSite));
            props.Add("_BonusCode", BonusCode);
            props.Add("_SiteIds", JsonConvert.SerializeObject(SiteList));
            props.Add("_PromotionType", PromotionType);
            props.Add("_Currency", CurrencyId);
            props.Add("_UserName", UserName);
            props.Add("_TransID", TransId);
            props.Add("_StartDate", StartDate);
            props.Add("_EndDate", EndDate);
            props.Add("_CategoryType", RebateCategoriesId);
            props.Add("_BaseCurrencyId", BaseCurrencyId);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_BonusInfoTotalAmount_Get_V4", props, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            CurrencyAmount = tempCurrencyTotalAmount;
            #endregion


            props.Add("_PageSize", PageSize);
            props.Add("_Startindex", Startindex);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            result = conn.Query<BonusInfo>("Admin_BonusInfo_Get_V7", props, commandType: CommandType.StoredProcedure);
            TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            RowCount = props.Get<int?>("_RowCount") ?? 0;


            return result;
        }

        public IEnumerable<BonusInfo> GetBonusInfoExportReport(string SelectSite, string BonusCode, string TransId, int CurrencyId, string UserName, int PromotionType, IEnumerable<int> SiteList, DateTime StartDate, DateTime EndDate, int BaseCurrencyId, int RebateCategoriesId)
        {
            IEnumerable<BonusInfo> result = null;
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var SpName = "Admin_BonusInfoExport_Get_V7";
            var props = new DynamicParameters();
            props.Add("_SelectSite", SelectSite);
            props.Add("_BonusCode", BonusCode);
            props.Add("_SiteIds", JsonConvert.SerializeObject(SiteList));
            props.Add("_PromotionType", PromotionType);
            props.Add("_Currency", CurrencyId);
            props.Add("_UserName", UserName);
            props.Add("_TransID", TransId);
            props.Add("_StartDate", StartDate);
            props.Add("_EndDate", EndDate);
            props.Add("_BaseCurrencyId", BaseCurrencyId);
            props.Add("_CategoryType", RebateCategoriesId);

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
            return conn.QueryFirstOrDefault<CancelBonus>("Admin_BonusCancel_Get",new { _BonusId = bonusId },commandType: CommandType.StoredProcedure);
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
                    result = walletService.SetBalance(custId, siteId, userName, backAmount, 7, $"Cancel Bonus({bonus.ToString("#,##0.##")}), Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out var afterAmount, out var is1stDepoist, conn, trans);

                    if (result > 0)
                        result = withdrawalLimitService.SetWithdrawalLimit(custId, userName, siteId, backAmount, $"Cancel Bonus, Return Amount: {backAmount.ToString("#,##0.##")}", bonusId.ToString(), out var afterWithdrawalLimit, conn, trans);
                } 
                
                //回收bonus紀錄
                var cancelBonusAmount = bonus - backAmount;
                if (cancelBonusAmount > 0 && result > 0 )
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
