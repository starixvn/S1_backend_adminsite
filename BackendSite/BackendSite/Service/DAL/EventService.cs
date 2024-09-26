using System.Data;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class EventService : IEventService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private MySqlConnection _connection;
        private MySqlTransaction _transaction;

        public EventService(IEnumerable<MySqlClient> sqlClients)
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
            await _transaction.DisposeAsync();
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        public async Task RollBack()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        /// <summary>
        /// 取得後台使用者設定的世界盃設定
        /// </summary>
        /// <param name="langId">語系Id，看前端傳什語系</param>
        /// <returns></returns>
        public IEnumerable<CupEventSettingDto> GetCupEventSetting(string langId)
        {
            var p = new DynamicParameters();
            p.Add("LangId", langId, dbType: DbType.String);
            return _mainDbClient.Query<CupEventSettingDto>("Admin_CupEventPredictionSetting_Get", p);
        }

        /// <summary>
        /// 取得世界盃國家下拉選單內容
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CupEventOptionsDto> GetCupEventOptions(string langId)
        {
            var p = new DynamicParameters();
            p.Add("LangId", langId, dbType: DbType.String);
            return _mainDbClient.Query<CupEventOptionsDto>("Admin_CupEventPredictionOptions_Get" ,p);
        }

        /// <summary>
        /// 更新 Status 狀態
        /// </summary>
        /// <returns></returns>
        public Task<int> SetCupEventPrediction()
        {
            return _mainDbClient.ExecuteAsync("Admin_CupEventPredictionSetting_Set", _transaction, null);
        }

        /// <summary>
        /// 寫入 CupEventSetting
        /// </summary>
        /// <param name="setData"></param>
        /// <returns></returns>
        public Task<int> InsertCupEventPrediction(CupEventSettingDto setData)
        {
            var parameters = new DynamicParameters();
            parameters.Add("_TeamId", setData.TeamId, dbType: DbType.Int64);
            parameters.Add("_LevelName", setData.LevelName, dbType: DbType.String);
            parameters.Add("_Sort", setData.Sort, dbType: DbType.Int64);
            return _mainDbClient.ExecuteAsync("Admin_CupEventPredictionSetting_Ins", _transaction, parameters);
        }

        public IEnumerable<CupEventMemberReportDto> GetMemberReport(GetCupEventMemberReportRequest request, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_TicketType", request.TicketType);
            p.Add("_TeamId", request.TeamId);
            p.Add("_Selected", request.Selected);
            p.Add("_UserName", request.UserName);
            p.Add("_StartTime", request.StartTime.ToUniversalTime().DateTime);
            p.Add("_EndTime", request.EndTime.ToUniversalTime().DateTime);
            p.Add("_PageSize", Math.Abs(request.PageSize));
            p.Add("_StartIndex", (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<CupEventMemberReportDto>("Admin_CupEventMemberTicketInfo_Get", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }

        public async Task<IEnumerable<CupEventMemberReportDto>> GetMemberReport(ExportCupEventMemberReportRequest request)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_TicketType", request.TicketType);
            p.Add("_TeamId", request.TeamId);
            p.Add("_Selected", request.Selected);
            p.Add("_UserName", request.UserName);
            p.Add("_StartTime", request.StartTime.ToUniversalTime().DateTime);
            p.Add("_EndTime", request.EndTime.ToUniversalTime().DateTime);
            return await _reportDbClient.QueryAsync<CupEventMemberReportDto>("Admin_CupEventMemberTicketInfoWithoutPage_Get", p);
        }

        public async Task<IEnumerable<CupEventPredictiveStatisticsDto>> GetPredictiveStatistics(int siteId)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            return await _reportDbClient.QueryAsync<CupEventPredictiveStatisticsDto>("Admin_CupEventPredictiveStatistics_Get", p);
        }

        public async Task<IEnumerable<CupEventPredictDetailDto>> GetPredictDetail(int siteId, int teamId)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_TeamId", teamId);
            return await _reportDbClient.QueryAsync<CupEventPredictDetailDto>("Admin_CupEventPredictDetail_Get", p);
        }

        public async Task<int> SetPayoutDone(int siteId, int teamId, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_TeamId", teamId);
            p.Add("UpdateOperator", updateOperator);
            return await _mainDbClient.ExecuteAsync("Admin_CupEventTeamSettleLog_Ins", p);
        }
        
        public async Task<IEnumerable<CupEventPredictScore>> GetCupEventPredictScore()
        {
            return await _reportDbClient.QueryAsync<CupEventPredictScore>("Admin_CupEventPredictScore_Get");
        }

        #region Lottery

        public IEnumerable<LotteryEventResponse> GetLotteryEventSetting(GetLotteryEventRequest request)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId, dbType: DbType.Int32);
            p.Add("_StartDate", request.StartDate, dbType: DbType.DateTime);
            p.Add("_EndDate", request.EndDate, dbType: DbType.DateTime);
            return _reportDbClient.Query<LotteryEventResponse>("Admin_LotteryEventDrawSetting_Get", p);
        }
        
        public LotteryEventResponse GetLotteryEventSettingBySq(int sq)
        {
            var p = new DynamicParameters();
            p.Add("_SQ", sq, dbType: DbType.Int32);
            return _reportDbClient.QueryFirst<LotteryEventResponse>("Admin_LotteryEventDrawSettingBySQ_Get", p);
        }
        
        public async Task<int> SetLotteryEventSetting(int sq, DateTime startDate , DateTime endDate , DateTime drawDate , int isSuspend , string excludeSetting ,string updator)
        {
            var p = new DynamicParameters();
            p.Add("_SQ", sq, dbType: DbType.Int32);
            p.Add("_StartDate", startDate, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate, dbType: DbType.DateTime);
            p.Add("_DrawDate", drawDate, dbType: DbType.DateTime);
            p.Add("_IsSuspend", isSuspend, dbType: DbType.String);
            p.Add("_ExcludeSetting", excludeSetting, dbType: DbType.String);
            p.Add("_Updator", updator, dbType: DbType.String);
            return await _mainDbClient.ExecuteAsync("Admin_LotteryEventDrawSetting_Set", p);
        }
        
        public async Task<int> AddLotteryEvent(int siteId, DateTime startDate , DateTime endDate , DateTime drawDate , string excludeSetting ,string updator)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_StartDate", startDate, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate, dbType: DbType.DateTime);
            p.Add("_DrawDate", drawDate, dbType: DbType.DateTime);
            p.Add("_ExcludeSetting", excludeSetting, dbType: DbType.String);
            p.Add("_Updator", updator, dbType: DbType.String);
            return await _mainDbClient.ExecuteAsync("Admin_LotteryEventDrawSetting_Ins", p);
        }
        
        public LotteryTicketHistory GetLotteryTicketHistory(int siteId ,int roundId)
        {
            return _reportDbClient.QueryFirst<LotteryTicketHistory>("Service_LotteryTicketHistory_Get", new { _SiteId = siteId , _RoundId = roundId });
        }

        public IEnumerable<RaffleTicketStickerResponse> GetRaffleTicketStickerSetting(GetRaffleTicketStickerRequest request)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId, dbType: DbType.Int32);
            p.Add("_Time", request.Time, dbType: DbType.DateTime);
            return _reportDbClient.Query<RaffleTicketStickerResponse>("Service_RaffleTicketStickerSetting_Get" , p);
        }
        
        public async Task<int> SetRaffleTicketStickerSetting(SetRaffleTicketStickerRequest request ,string updator)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId, dbType: DbType.Int32);
            p.Add("_CurrencyId", request.CurrencyId, dbType: DbType.Int32);
            p.Add("_StickerPrize", request.StickerPrize, dbType: DbType.Decimal);
            p.Add("_TurnOver", request.TurnOver, dbType: DbType.Decimal);
            p.Add("_Updator", updator, dbType: DbType.String);
            return await _mainDbClient.ExecuteAsync("Admin_RaffleTicketSticker_Set", p);
        }
        
        public IEnumerable<EventRestrictionResponse> GetEventRestrictionList(GetEventRestrictionRequest request)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId, dbType: DbType.Int32);
            p.Add("_CurrencyId", request.CurrencyId, dbType: DbType.Int32);
            p.Add("_UserName", request.UserName, dbType: DbType.String);
            return _reportDbClient.Query<EventRestrictionResponse>("Admin_EventRestriction_Get" , p);
        }
        
        public async Task<int> InsEventRestriction(int siteId, int currencyId, int custId ,string reason ,string creator)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_CurrencyId", currencyId, dbType: DbType.Int32);
            p.Add("_CustId", custId, dbType: DbType.Int32);
            p.Add("_Reason", reason, dbType: DbType.String);
            p.Add("_Creator", creator, dbType: DbType.String);
            return await _mainDbClient.ExecuteAsync("Admin_EventRestriction_Ins", p);
        }
        
        public async Task<int> DelEventRestriction(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int32);
            return await _mainDbClient.ExecuteAsync("Admin_EventRestriction_Del", p);
        }

        public async Task<GetStickerReportResponse> GetStickerReport(GetStickerReportRequest request)
        {
            var result = new GetStickerReportResponse
            {
                StickerReportData = new List<GetStickerReportDto>()
            };

            var p = new DynamicParameters(new
            {
                _SiteList = JsonConvert.SerializeObject(request.SiteIds),
                _CurrencyList = JsonConvert.SerializeObject(request.CurrencyIds),
                _UserName = request.UserName,
                _RoundId = int.TryParse(request.RoundId, out var roundId) ? roundId : 0,
                _StartTime = request.StartDate,
                _EndTime = request.EndDate,
                _PageNumber = request.PageNumber
            });

            var startIndex = (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize);
            // if (startIndex == 0)
            // {
            //     startIndex = 1;
            // }
            p.Add("_PageSize", request.PageSize, DbType.Int32, ParameterDirection.Input);
            p.Add("_StartIndex", startIndex, DbType.Int32, ParameterDirection.Input);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var getData = await _mainDbClient.QueryAsync<GetStickerReportDto>("Admin_StickerReport_Get" , p);

            if (getData.Count() != 0)
            {
                result.StickerReportData.AddRange(getData);
            }
            
            result.TotalPages = p.Get<int?>("_TotalPages") ?? 0;
            result.RowCount = p.Get<int?>("_RowCount") ?? 0;
            result.PageNumber = request.PageNumber;
            
            return result;
        }
        
        public IEnumerable<GetCustLotteryTicket> GetCustLotteryTicket(int siteId , string currencyIds, string userName , int roundId ,string ticketNo)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId, dbType: DbType.Int32);
            p.Add("_CurrencyIds", currencyIds, dbType: DbType.String);
            p.Add("_UserName", userName, dbType: DbType.String);
            p.Add("_RoundId", roundId, dbType: DbType.Int32);
            p.Add("_TicketNo", ticketNo, dbType: DbType.String);
            return _reportDbClient.Query<GetCustLotteryTicket>("Admin_CustLotteryTicket_Get", p);
        }
        
        #endregion
        
    }
}
