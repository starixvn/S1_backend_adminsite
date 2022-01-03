using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class GameCasinoManageService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public GameCasinoManageService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public int SetGameCasinoDetail(SetGameCasinoPara para)
        {
            var props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            props.Add("Type", para.Type switch
            {
                "hot" => 0,
                "featured" => 1,
                "status" => 2,
                _ => 3
            });
            return _mainDbClient.Execute("Admin_GameCasinoDetail_Set", props);
        }

        public int InsertGameCasinoDetail (InsertGameCasinoPara para) 
        {
            var props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            return _mainDbClient.Execute("Admin_GameCasinoDetail_Ins", props);
        }

        public int DeleteGameCasinoDetail(DeleteGameCasinoPara para)
        {
            var props = new DynamicParameters();
            props.Add("SiteId", para.SiteId);
            props.Add("CurrencyId", para.CurrencyId);
            props.Add("GameCasinoId", para.GameCasinoId);
            return _mainDbClient.Execute("Admin_GameCasinoDetail_Del", props);
        }

        public IEnumerable<SiteTpListIfo> GetTpList()
        {
            return _reportDbClient.Query<SiteTpListIfo>("Admin_TpList_Get");
        }

        public GameCasinoResult GameCasinoByTPIdV2(GameCasinoPara gameCasinoPara)
        {
            var logResult = new GameCasinoResult();
            var props = new DynamicParameters();
            props.Add("PageNumber", gameCasinoPara.PageNumber, DbType.Int32,ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", gameCasinoPara.PageSize);
            props.Add("TPId", gameCasinoPara.TPid);
            props.Add("CategoryId", gameCasinoPara.Categories);
            props.Add("SiteId", gameCasinoPara.Site);
            props.Add("CurrencyId", gameCasinoPara.Currency);
            props.Add("Names", string.IsNullOrEmpty(gameCasinoPara.Name?.Replace(" ", "")) ? null : $"%{gameCasinoPara.Name.Trim()}%");

            var result = _mainDbClient.Query<GameCasinoData>("Admin_GameCasinoV3_Get", props);
            logResult.GameCasinoData = result.ToList();
            logResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            logResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            logResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return logResult;
        }

        public int SetGameCasino(SetGameCasionV2Para para)
        {
            var props = new DynamicParameters();
            props.Add("GameCasinoId", para.GameCasinoId);
            props.Add("Type", para.Type switch
            {
                "hot" => 0,
                "featured" => 1,
                "status" => 2,
                _ => 3
            });
            return _mainDbClient.Execute("Admin_GameCasino_Set", props);
        }

        public IEnumerable<CurrencyDataV2> GetCurrency(GetGameCasinoDetailPara para)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
            props.Add("GameCasinoId", para.GameCasinoId);
            return conn.Query<CurrencyDataV2>("Admin_Currency_GetByCasinoId", props, commandType: CommandType.StoredProcedure);
        }
        public List<GameCasinoDetail> GetGameCasinoDetailBySiteAndCasinoIdV2(GetGameCasinoDetailPara para,int SiteId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            var props = new DynamicParameters();
            props.Add("GameCasinoId", para.GameCasinoId);
            props.Add("SiteId", SiteId);
            return conn.Query<GameCasinoDetail>("Admin_GameCasinoDetail_GetBySiteAndCasinoId", props, commandType: CommandType.StoredProcedure).ToList();
        }
    }
}
