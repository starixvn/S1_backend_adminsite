using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BackendSite.Service.DAL
{
    public class PromotionService
    {
        private readonly IConfiguration configuration;

        public PromotionService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<Promotion> GetPromotionListByCustId(int custId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Promotion>("Admin_PromotionListByCustId_Get_V2", new { _CustId = custId }, commandType: CommandType.StoredProcedure);
        }

        //public int CancelBonusByBonusId(int bonusId)
        //{
        //    using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
        //    conn.Open();
        //    using var trans = conn.BeginTransaction();
        //    int result = 0;
        //    result = conn.Execute("Common_CancelBonusByBonusId_Set", new { BonusId = bonusId }, transaction: trans, commandType: CommandType.StoredProcedure);
        //    trans.Commit();
        //    return result;
        //}

        public PromotionListData GetPromotionListByCustIdAndDate(int CustId, DateTimeOffset StartDate, DateTimeOffset EndDate, int PageNumber, int PageSize)
        {
            var PromotionListData = new PromotionListData();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var tempCurrencyTotalAmount = new List<string>();

            var props = new DynamicParameters();
            props.Add("_CustId", CustId, DbType.Int32);
            props.Add("_StartDate", StartDate.DateTime, DbType.DateTime);
            props.Add("_EndDate", EndDate.DateTime, DbType.DateTime);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_PromotionTotalAmountByCustIdAndDate_Get_V3", props, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            PromotionListData.CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            props.Add("_StartIndex", (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize));
            props.Add("_PageSize", Math.Abs(PageSize));
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = conn.Query<Promotion>("Admin_PromotionListByCustIdAndDate_Get_V4", props, commandType: CommandType.StoredProcedure).ToList();

            PromotionListData.PromotionList = result;
            PromotionListData.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            PromotionListData.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return PromotionListData;
        }
    }
}
