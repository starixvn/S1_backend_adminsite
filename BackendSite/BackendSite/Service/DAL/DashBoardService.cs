using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class DashBoardService
    {
        private readonly IConfiguration configuration;

        public DashBoardService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public DashBoardData GetDashBoardData(int SiteId, int CurrencyId, int BaseCurrencyId)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("SiteId", SiteId);
            props.Add("CurrencyId", CurrencyId);
            props.Add("BaseCurrencyId", BaseCurrencyId);

            return conn.Query<DashBoardData>("Admin_LatestHighlightVideo_Get", props, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }
    }
}
