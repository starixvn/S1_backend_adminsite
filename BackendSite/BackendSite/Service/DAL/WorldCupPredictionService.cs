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
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private MySqlConnection _connection;
        private MySqlTransaction _transaction;

        public WorldCupPredictionService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
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

        public IEnumerable<WorldCupMemberReportDto> GetMemberReport(GetWorldCupMemberReportRequest request, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_TicketType", request.TicketType);
            p.Add("_TeamId", request.TeamId);
            p.Add("_Selected", request.Selected);
            p.Add("_UserName", request.UserName);
            p.Add("_StartTime", request.StartTime);
            p.Add("_EndTime", request.EndTime);
            p.Add("_PageSize", Math.Abs(request.PageSize));
            p.Add("_StartIndex", (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<WorldCupMemberReportDto>("Admin_WorldCupMemberTicketInfo_Get", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }

        public async Task<IEnumerable<WorldCupMemberReportDto>> GetMemberReport(ExportWorldCupMemberReportRequest request)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_TicketType", request.TicketType);
            p.Add("_TeamId", request.TeamId);
            p.Add("_Selected", request.Selected);
            p.Add("_UserName", request.UserName);
            p.Add("_StartTime", request.StartTime);
            p.Add("_EndTime", request.EndTime);
            return await _reportDbClient.QueryAsync<WorldCupMemberReportDto>("Admin_WorldCupMemberTicketInfoWithoutPage_Get", p);
        }

        public async Task<IEnumerable<WorldCupPredictiveStatisticsDto>> GetPredictiveStatistics(int siteId)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            return await _reportDbClient.QueryAsync<WorldCupPredictiveStatisticsDto>("Admin_WorldCupPredictiveStatistics_Get", p);
        }
        
        public async Task<IEnumerable<WorldCupPredictDetailDto>> GetPredictDetail(int siteId, int teamId)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_TeamId", teamId);
            return await _reportDbClient.QueryAsync<WorldCupPredictDetailDto>("Admin_WorldCupPredictDetail_Get", p);
        }

        public async Task<int> SetPayoutDone(int siteId, int teamId, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_TeamId", teamId);
            p.Add("UpdateOperator", updateOperator);
            return await _mainDbClient.ExecuteAsync("Admin_WorldCupTeamSettleLog_Ins", p);
        }
    }
}
