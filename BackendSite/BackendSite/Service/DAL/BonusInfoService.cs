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

        public BonusInfoService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<BonusInfo> GetBonusInfoReport(string TransId, int CurrencyId, string UserName, int PromotionType, IEnumerable<int> SiteList, long StartDate, long EndDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            IEnumerable<BonusInfo> result = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_BonusInfo_Get";
                DynamicParameters props = new DynamicParameters();
                props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

                props.Add("@PageSize", PageSize);

                props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));
                props.Add("@PromotionType", PromotionType);
                props.Add("@Currency", CurrencyId);
                props.Add("@UserName", UserName);
                props.Add("@TransID", TransId);
                
                props.Add("@StartDate", StartDate);
                props.Add("@EndDate", EndDate);
                

                result = conn.Query<BonusInfo>(SpName, props, commandType: CommandType.StoredProcedure);
                PageNumber = props.Get<int?>("@PageNumber") ?? 0;
                TotalPages = props.Get<int?>("@TotalPages") ?? 0;
                RowCount = props.Get<int?>("@RowCount") ?? 0;
            }

            return result;
        }

        public IEnumerable<BonusInfo> GetBonusInfoExportReport(string TransId, int CurrencyId, string UserName, int PromotionType, IEnumerable<int> SiteList, long StartDate, long EndDate)
        {
            IEnumerable<BonusInfo> result = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_BonusInfoExport_Get";
                DynamicParameters props = new DynamicParameters();
                props.Add("@SiteIds", JsonConvert.SerializeObject(SiteList));
                props.Add("@PromotionType", PromotionType);
                props.Add("@Currency", CurrencyId);
                props.Add("@UserName", UserName);
                props.Add("@TransID", TransId);
                props.Add("@StartDate", StartDate);
                props.Add("@EndDate", EndDate);

                result = conn.Query<BonusInfo>(SpName, props, commandType: CommandType.StoredProcedure);
            }

            return result;
        }
    }
}
