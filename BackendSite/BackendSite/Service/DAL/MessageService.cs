using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BatchMessageSendLog;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class MessageService
    {
        private readonly IConfiguration _configuration;
        private readonly string _commonServiceUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly ILogger<MessageService> _logger;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public MessageService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _commonServiceUrl = _configuration.GetValue<string>("CommonService");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _logger = provider.GetRequiredService<ILogger<MessageService>>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public int SendMessageToPlayer(int siteId, string sender, string title, string content, int custId, string CustUserName, string lang)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Msg_Ins", new { Type = 99, siteId = siteId, LangId = lang, RecipientCustId = custId, Recipient = CustUserName, SenderCustId = siteId, Sender = sender, Title = title, Content = content, MainId = 0 }, commandType: CommandType.StoredProcedure);
        }

        public int SendMessageToPlayer(int siteId, string sender, string title, string content, int custId, string CustUserName, string lang, int SenderCustId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Msg_Ins", new { Type = 99, siteId = siteId, LangId = lang, RecipientCustId = custId, Recipient = CustUserName, SenderCustId = SenderCustId, Sender = sender, Title = title, Content = content, MainId = 0 }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Announcement> GetAnnouncements(string selectSite,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int status,
            int startIndex,
            int pageSize,
            ref int totalPages,
            ref int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("SelectSite", selectSite);
            props.Add("StartDate", startDate.DateTime,DbType.DateTime);
            props.Add("EndDate", endDate.DateTime,DbType.DateTime);
            props.Add("Status", status, DbType.Int32);
            props.Add("StartIndex", startIndex, DbType.Int32);
            props.Add("PageSize", pageSize);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<Announcement>("Admin_AnnouncementV2_Get", props);
            totalPages = props.Get<int?>("TotalPages") ?? 0;
            rowCount = props.Get<int?>("RowCount") ?? 0;

            return result;
        }

        public Announcement GetAnnouncementById(int id)
        {
            return _mainDbClient.QueryFirst<Announcement>("Admin_Announcement_GetById", new DynamicParameters(new {Id = id}));
        }
        
        public Announcement AddAnnouncement(AddAnnouncementModel data, string creator, int creatorId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var result = new Announcement();
            var param = new DynamicParameters();
            param.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
            param.Add("@SiteId", data.SiteId);
            param.Add("@StartTime", startTime.DateTime);
            param.Add("@EndTime", endTime.DateTime);
            param.Add("@Subject", data.Subject);
            param.Add("@Content", data.Content);
            param.Add("@Status", data.Status);
            param.Add("@Creator", creator);
            param.Add("@CreatorId", creatorId);
            param.Add("@IsTest", data.IsTest);

            _mainDbClient.Execute("Admin_Announcement_Ins", param);

            var id = param.Get<int?>("@Id");
            if (id != null)
            {
                result = _mainDbClient.QueryFirst<Announcement>("Admin_Announcement_GetById", new DynamicParameters(new {Id = id }));
            }
            return result;
        }

        public Announcement UpdateAnnouncement(UpdateAnnouncementModel data, string lastUpdater, int lastUpdaterId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var param = new DynamicParameters();
            param.Add("@Id", data.Id);
            param.Add("@SiteId", data.SiteId);
            param.Add("@StartTime", startTime.DateTime);
            param.Add("@EndTime", endTime.DateTime);
            param.Add("@Subject", data.Subject);
            param.Add("@Content", data.Content);
            param.Add("@Status", data.Status);
            param.Add("@LastUpdater", lastUpdater);
            param.Add("@LastUpdaterId", lastUpdaterId);
            param.Add("@IsTest", data.IsTest);

            _mainDbClient.Execute("Admin_Announcement_Upd", param);
            var result = _mainDbClient.QueryFirst<Announcement>("Admin_Announcement_GetById",new DynamicParameters(new { data.Id }));

            return result;
        }

        public int DeleteAnnouncement(int id)
        {
            return _mainDbClient.Execute("Admin_Announcement_Del", new DynamicParameters(new {Id = id}));
        }

        public IEnumerable<PersonalMsg> GetPersonalMsg(int CustId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            IEnumerable<PersonalMsg> result = null;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            var SpName = "Admin_PersonalMessage_Get";
            var props = new DynamicParameters();
            props.Add("@CustId", CustId, dbType: DbType.Int32);
            props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@PageSize", PageSize);

            result = conn.Query<PersonalMsg>(SpName, props, commandType: CommandType.StoredProcedure);
            PageNumber = props.Get<int?>("@PageNumber") ?? 0;
            TotalPages = props.Get<int?>("@TotalPages") ?? 0;
            RowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }

        public CustLang GetCustLang(int CustId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Query<CustLang>("Admin_CustLang_Get", new { CustId }, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        public void DeletePersonalMsg(int Id, int CustId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Execute("Admin_PersonalMsg_Del", new { Id, CustId }, commandType: CommandType.StoredProcedure);
        }
        public bool SendMessageBatch(string personalMsg, int custId, int siteId, string sender, int batchId)
        {
            using var connection = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            connection.Open();
            using var trans = connection.BeginTransaction();
            var result = _mainDbClient.Execute("Admin_Msg_BatchV2_Ins",new DynamicParameters(new { Sender = sender, CustId = custId, SiteId = siteId, PersonalMsg = personalMsg, BatchId = batchId }) ,trans);

            if (result == 1)
            {
                trans.Commit();
                return true;
            } 
            trans.Rollback();
            return false;
        }
        public int ChangeVipLevelBatch(int siteId, int custId, string userName, int vipLevel, string memo, string Operator)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_Change_VipLevel_Batch", new { SiteId = siteId, CustId = custId, UserName = userName, VipLevel = vipLevel, Memo = memo, Operator = Operator }, commandType: CommandType.StoredProcedure);
        }
        public bool NewBatchMessage(int siteId, string author, string personalMsg, ref int batchId)
        {
            var p = new DynamicParameters(new { SiteId = siteId, Author = author, Message = personalMsg });
            p.Add("BatchId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _mainDbClient.Execute("Admin_BatchMessage_Ins", p);
            batchId = p.Get<int>("BatchId");
            return result > 0;
        }
        public bool DeleteBatchMessage(int batchId)
        {
            _logger.LogInformation($@"MessageService-DeleteBatchMessage Start.");
            
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var bp = new DynamicParameters();
            bp.Add("_BatchId", batchId);
            var batchMessageCount = conn.QueryFirst<int>("Admin_BatchMessageForDelete_Get", bp, trans, commandType: CommandType.StoredProcedure);
            _logger.LogInformation($@"MessageService-DeleteBatchMessage Expect Delete Message Count={batchMessageCount}.");

            if (batchMessageCount > 0)
            {
                var affectCount = conn.QueryFirstOrDefault<int>("Admin_BatchMessageV2_Del", bp, trans, commandType: CommandType.StoredProcedure);
                if (affectCount != batchMessageCount)
                {
                    trans.Rollback();
                    _logger.LogError($@"MessageService-DeleteBatchMessage Expect Delete Batch Message Count={batchMessageCount},Actual Delete Batch Message Count={affectCount}.Transaction rollback.");
                    return false;
                }
            }

            var p = new DynamicParameters();
            p.Add("_BatchId", batchId);
            var messageCount = conn.QueryFirstOrDefault<int>("Admin_MessageForDelete_Get", p, trans, commandType: CommandType.StoredProcedure);
            _logger.LogInformation($@"MessageService-DeleteBatchMessage Expect Delete Message Count={messageCount}.");

            if (messageCount > 0)
            {
                var affectCount = conn.QueryFirstOrDefault<int>("Admin_MessageByBatchId_Del", bp, trans, commandType: CommandType.StoredProcedure);
                if (affectCount != messageCount)
                {
                    trans.Rollback();
                    _logger.LogError($@"MessageService-DeleteBatchMessage Expect Delete Message Count={messageCount},Actual Delete Message Count={affectCount}.Transaction rollback.");
                    return false;
                }
            }
            trans.Commit();
            _logger.LogInformation($@"MessageService-DeleteBatchMessage End.");
            return true;
        }
        public bool UpdateBatchMessage(int batchId, string message)
        {
            var props = new DynamicParameters();
            props.Add("BatchId", batchId);
            props.Add("Message", message);
            return _mainDbClient.Execute("Admin_BatchMessageUpdate_Set", props) > 0;
        }
        public BatchMsg GetBatchMessage(int batchId)
        {
            var p = new DynamicParameters(new { BatchId = batchId });
            var result = _mainDbClient.QueryFirst<BatchMsg>("Admin_BatchMessage_Get", p);
            return result;
        }
        public IEnumerable<BatchMsg> GetBatchMessageList(IEnumerable<int> selectSite, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("@SelectSite", JsonConvert.SerializeObject(selectSite));
            props.Add("@PageNumber", Math.Abs(pageNumber),DbType.Int32, ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@PageSize", Math.Abs(pageSize));
            props.Add("@StartDate", startDate.DateTime);
            props.Add("@EndDate", endDate.DateTime);

            var result = _mainDbClient.Query<BatchMsg>("Admin_BatchMessageList_Get", props);
            pageNumber = props.Get<int?>("@PageNumber") ?? 0;
            totalPages = props.Get<int?>("@TotalPages") ?? 0;
            rowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }
        public async Task<ApiResult> SendSMSMessageToPlayerAsync(int CustId)
        {
            var apiUrl = $"{_commonServiceUrl}OTPSMS/SendSMS";
            ApiResult apiResult = await _courierAdapter.MyPostAsync<IEnumerable<FireBaseReturn>, dynamic>(apiUrl, new { CustId });
            return apiResult;
        }

        public bool InsertBatchMessageSendLog(int batchId,
            int rowId,
            int custId,
            string userName,
            string sabaId,
            string resultMessage,
            string updator,
            int status)
        {
            var props = new DynamicParameters();
            props.Add("_BatchId", batchId);
            props.Add("_Seq", rowId);
            props.Add("_CustId", custId);
            props.Add("_UserName", userName);
            props.Add("_SabaId", sabaId);
            props.Add("_Result", resultMessage);
            props.Add("_Creator", updator);
            props.Add("_Status", status);

            return _mainDbClient.Execute("Admin_BatchMessageSendLog_Ins", props) == 1;
        }

        public IEnumerable<BatchMessageSendLogListModel> GetBatchMessageSendLogResult(int batchId)
        {
            var props = new DynamicParameters();
            props.Add("_BatchId", batchId);
            return _reportDbClient.Query<BatchMessageSendLogListModel>("Admin_BatchMessageSendLog_Get", props);
        }
    }
}
