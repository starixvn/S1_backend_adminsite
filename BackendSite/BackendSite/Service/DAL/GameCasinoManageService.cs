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

        public GameCasinoResult GameCasinoByTPId(GameCasinoPara gameCasinoPara)
        {
            GameCasinoResult LogResult = new GameCasinoResult();
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("PageNumber", gameCasinoPara.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", gameCasinoPara.PageSize);
            props.Add("TPId", gameCasinoPara.TPid);
            props.Add("SiteId", gameCasinoPara.Site);
            props.Add("CurrencyId", gameCasinoPara.Currency);

            var result = conn.Query<GameCasinoData>("Admin_GameCasino_Get", props, commandType: CommandType.StoredProcedure);
            LogResult.GameCasinoData = result.ToList();
            LogResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            LogResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            LogResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return LogResult;
        }

        public GameCasinoDetail GetGameCasinoDetail(GameCasinoDetailPara para)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            return conn.Query<GameCasinoDetail>("Admin_GameCasinoDetail_Get", props, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }
        public int SetGameCasinoDetail(SetGameCasionPara para)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            props.Add("Hot", para.Hot ? 1 : 2);
            props.Add("Featured", para.Featured ? 1 : 2);
            props.Add("Status", para.Status ? 1 : 2);
            return conn.Execute("Admin_GameCasinoDetail_Set", props, commandType: CommandType.StoredProcedure);
        }
        public int InsertGameCasinoDetail (InsertGameCasionPara para) 
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            return conn.Execute("Admin_GameCasinoDetail_Ins", props, commandType: CommandType.StoredProcedure);
        }
        public int DeleteGameCasinoDetail(DeleteGameCasionPara para)
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            return conn.Execute("Admin_GameCasinoDetail_Del", props, commandType: CommandType.StoredProcedure);
        }
    }
}
