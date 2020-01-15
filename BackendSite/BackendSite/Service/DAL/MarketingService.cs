using System.Collections.Generic;
using System.Data;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.DAL
{
    public class MarketingService
    {
        private readonly IConfiguration configuration;

        public MarketingService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<PointLevelInfo> GetPointLevelInfos(int siteId, int currencyId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<PointLevelInfo>("PointLevelInfo_Get", new { SiteId = siteId, CurrencyId = currencyId }, commandType: CommandType.StoredProcedure);
            }
        }

        public bool AddPointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var param = new
                {
                    pointLevelInfo.SiteId,
                    pointLevelInfo.CurrencyId,
                    pointLevelInfo.PointLevel,
                    pointLevelInfo.DepositLeast,
                    pointLevelInfo.BetLeast
                };
                return conn.Execute("PointLevelInfo_Ins", param, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public bool EditPointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var param = new
                {
                    pointLevelInfo.SiteId,
                    pointLevelInfo.CurrencyId,
                    pointLevelInfo.PointLevel,
                    pointLevelInfo.DepositLeast,
                    pointLevelInfo.BetLeast
                };
                return conn.Execute("PointLevelInfo_Set", param, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public PointLevelInfo GetPointLevelInfo(int siteId, int currencyId, int point)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<PointLevelInfo>("PointLevelInfoDetail_Get", new { SiteId = siteId, CurrencyId = currencyId, PointLevel = point }, commandType: CommandType.StoredProcedure);
            }
        }

        public int DeletePointLevelInfo(PointLevelInfo pointLevelInfo)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("PointLevelInfo_Del",
                                                new { SiteId = pointLevelInfo.SiteId, CurrencyId = pointLevelInfo.CurrencyId, PointLevel = pointLevelInfo.PointLevel },
                                                commandType: CommandType.StoredProcedure);
            }
        }
    }
}