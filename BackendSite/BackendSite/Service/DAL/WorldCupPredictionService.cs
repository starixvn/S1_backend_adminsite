using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.DAL
{
    public class WorldCupPredictionService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private MySqlConnection _connection;
        private MySqlTransaction _transaction;

        public WorldCupPredictionService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }

        public async Task BeginTransaction()
        {
            _connection = _mainDbClient.GetMySqlConnection();
            await _connection.OpenAsync();
            _transaction = await _connection.BeginTransactionAsync();
        }

        public async Task Commit()
        {
            await _transaction.CommitAsync();
            await _connection.CloseAsync();
        }

        public async Task RollBack()
        {
            await _transaction.RollbackAsync();
            await _connection.CloseAsync();
        }

        /// <summary>
        /// 取得後台使用者設定的世界盃設定
        /// </summary>
        /// <param name="langId">語系Id，看前端傳什語系</param>
        /// <returns></returns>
        public IEnumerable<WorldCupPredictionSettingDto> GetWorldCupPredictionSetting(string langId)
        {
            var p = new DynamicParameters();
            p.Add("LangId", langId, dbType: DbType.String);
            return _mainDbClient.Query<WorldCupPredictionSettingDto>("Admin_WorldCupPredictionSetting_Get", p);
        }

        /// <summary>
        /// 取得世界盃國家下拉選單內容
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WorldCupPredictionOptionsDto> GetWorldCupPredictionOptions()
        {
            return _mainDbClient.Query<WorldCupPredictionOptionsDto>("Admin_WorldCupPredictionOptions_Get");
        }

        /// <summary>
        /// 更新 Status 狀態
        /// </summary>
        /// <returns></returns>
        public Task<int> SetWorldCupPrediction()
        {
            return _mainDbClient.ExecuteAsync("Admin_WorldCupPredictionSetting_Set", null, _connection, _transaction);
        }

        /// <summary>
        /// 寫入 WorldCupEventSetting
        /// </summary>
        /// <param name="setData"></param>
        /// <returns></returns>
        public Task<int> InsertWorldCupPrediction(WorldCupPredictionSettingDto setData)
        {
            var parameters = new DynamicParameters();
            parameters.Add("_TeamId", setData.TeamId, dbType: DbType.Int64);
            parameters.Add("_LevelName", setData.LevelName, dbType: DbType.String);
            parameters.Add("_Sort", setData.Sort, dbType: DbType.Int64);
            return _mainDbClient.ExecuteAsync("Admin_WorldCupPredictionSetting_Ins", parameters, _connection, _transaction);
        }
    }
}
