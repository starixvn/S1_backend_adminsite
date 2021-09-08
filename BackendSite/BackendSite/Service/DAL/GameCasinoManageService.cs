using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class GameCasinoManageService
    {
        private readonly IConfiguration configuration;

        public GameCasinoManageService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public int SetGameCasinoDetail(SetGameCasionPara para)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            props.Add("Type", para.Type == "hot" ? 0 : para.Type == "featured" ? 1 : para.Type == "status" ? 2:3);
            return conn.Execute("Admin_GameCasinoDetail_Set", props, commandType: CommandType.StoredProcedure);
        }

        public int InsertGameCasinoDetail (InsertGameCasionPara para) 
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            return conn.Execute("Admin_GameCasinoDetail_Ins", props, commandType: CommandType.StoredProcedure);
        }

        public int DeleteGameCasinoDetail(DeleteGameCasionPara para)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            return conn.Execute("Admin_GameCasinoDetail_Del", props, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<SiteTpListIfo> GetTpList()
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                return conn.Query<SiteTpListIfo>("Admin_TpList_Get", new {}, commandType: CommandType.StoredProcedure);
            }
        }

        public GameCasinoResult GameCasinoByTPIdV2(GameCasinoPara gameCasinoPara)
        {
            var LogResult = new GameCasinoResult();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var props = new DynamicParameters();
            props.Add("PageNumber", gameCasinoPara.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", gameCasinoPara.PageSize);
            props.Add("TPId", gameCasinoPara.TPid);
            props.Add("CategoryId", gameCasinoPara.Categories);
            props.Add("SiteId", gameCasinoPara.Site);
            props.Add("CurrencyId", gameCasinoPara.Currency);
            props.Add("Names", string.IsNullOrEmpty(gameCasinoPara.Name == null ? null : gameCasinoPara.Name.Replace(" ", "")) ? null : $"%{gameCasinoPara.Name.Trim()}%");

            var result = conn.Query<GameCasinoData>("Admin_GameCasinoV3_Get", props, commandType: CommandType.StoredProcedure);
            LogResult.GameCasinoData = result.ToList();
            LogResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            LogResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            LogResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return LogResult;
        }

        public int SetGameCasino(SetGameCasionV2Para para)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var props = new DynamicParameters();
            props.Add("GameCasinoId", para.GameCasinoId);
            props.Add("Type", para.Type == "hot"? 0: para.Type == "featured"? 1: para.Type == "status"? 2: 3);
            return conn.Execute("Admin_GameCasino_Set", props, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<CurrencyDataV2> GetCurrency(GetGameCasinoDetailPara para)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("GameCasinoId", para.GameCasinoId);
            return conn.Query<CurrencyDataV2>("Admin_Currency_GetByCasinoId", props, commandType: CommandType.StoredProcedure);
        }
        public List<GameCasinoDetail> GetGameCasinoDetailBySiteAndCasinoIdV2(GetGameCasinoDetailPara para,int SiteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var props = new DynamicParameters();
            props.Add("GameCasinoId", para.GameCasinoId);
            props.Add("SiteId", SiteId);
            return conn.Query<GameCasinoDetail>("Admin_GameCasinoDetail_GetBySiteAndCasinoId", props, commandType: CommandType.StoredProcedure).ToList();
        }
    }
}
