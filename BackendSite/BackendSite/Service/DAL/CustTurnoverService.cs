using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class CustTurnoverService
    {
        private readonly IConfiguration configuration;
        public CustTurnoverService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<CustTurnover> GetCustTurnover(int CustId, string[]GameTypes, int StartTime, int EndTime)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustTurnover>("Admin_TpBetRecord_GetByCustId", new { 
                CustId = CustId, 
                GameList=Newtonsoft.Json.JsonConvert.SerializeObject(GameTypes),
                StartDate=StartTime,
                EndDate=EndTime
            }, commandType: CommandType.StoredProcedure);
        }
    }
}
