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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var tempCurrencyTotalAmount = new List<string>();

            var props = new DynamicParameters();
            props.Add("CustId", CustId,DbType.Int32);
            props.Add("StartDate", StartDate.DateTime, DbType.DateTime);
            props.Add("EndDate", EndDate.DateTime, DbType.DateTime);

            #region GetCurrencyTotalAmount
            var CurrencyTotalAmount = conn.Query<TotalAmount>("Admin_PromotionTotalAmountByCustIdAndDate_Get_V2", props, commandType: CommandType.StoredProcedure).ToList();
            if (CurrencyTotalAmount.Count > 0)
            {
                CurrencyTotalAmount.ForEach(x =>
                {
                    tempCurrencyTotalAmount.Add(string.Format("{0}:{1}", x.Currency, Math.Round(x.Amount, 2)));
                });
            }
            PromotionListData.CurrencyAmount = tempCurrencyTotalAmount;
            #endregion

            props.Add("PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", PageSize);
            var result =  conn.Query<Promotion>("Admin_PromotionListByCustIdAndDate_Get_V3", props, commandType: CommandType.StoredProcedure).ToList();

            PromotionListData.PromotionList = result;
            PromotionListData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            PromotionListData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            PromotionListData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return PromotionListData;
        }
    }
}
