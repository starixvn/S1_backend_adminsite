using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class MessageService
    {
        private readonly IConfiguration configuration;
        private readonly string commonServiceUrl;
        private readonly CourierAdapter courierAdapter;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IConfiguration configuration,
            CourierAdapter courierAdapter,
            ILogger<MessageService> logger)
        {
            this.configuration = configuration;
            commonServiceUrl = configuration.GetValue<string>("CommonService");
            this.courierAdapter = courierAdapter;
            _logger = logger;
        }

        public int SendMessageToPlayer(int siteId, string sender, string title, string content, int custId, string CustUserName, string lang)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Msg_Ins", new { Type = 99, siteId = siteId, LangId = lang, RecipientCustId = custId, Recipient = CustUserName, SenderCustId = siteId, Sender = sender, Title = title, Content = content, MainId = 0 }, commandType: CommandType.StoredProcedure);
        }

        public int SendMessageToPlayer(int siteId, string sender, string title, string content, int custId, string CustUserName, string lang, int SenderCustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Msg_Ins", new { Type = 99, siteId = siteId, LangId = lang, RecipientCustId = custId, Recipient = CustUserName, SenderCustId = SenderCustId, Sender = sender, Title = title, Content = content, MainId = 0 }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Announcement> GetAnnouncements(IEnumerable<int> selectSite, long startDate, long endDate, int status, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            IEnumerable<Announcement> result = null;
            //site為ALL類型的都可以查
            List<int> tempselectSite = selectSite.ToList();
            tempselectSite.Add(0);
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Admin_AnnouncementV2_Get";
                DynamicParameters props = new DynamicParameters();
                props.Add("SelectSite", JsonConvert.SerializeObject(tempselectSite));
                props.Add("StartDate", startDate);
                props.Add("EndDate", endDate);
                props.Add("Status", status);
                props.Add("PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("PageSize", PageSize);

                result = conn.Query<Announcement>(SpName, props, commandType: CommandType.StoredProcedure);
                PageNumber = props.Get<int?>("@PageNumber") ?? 0;
                TotalPages = props.Get<int?>("@TotalPages") ?? 0;
                RowCount = props.Get<int?>("@RowCount") ?? 0;
            }

            return result;
        }

        public Announcement GetAnnouncementById(int Id)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Announcement>("Admin_Announcement_GetById", new { Id }, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        public Announcement AddAnnouncement(Announcement Datas, string Creator, int CreatorId, long _StartTime, long _EndTime)
        {
            Announcement result = new Announcement();
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    string SpName = "Admin_Announcement_Ins";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    param.Add("@SiteId", Datas.SiteId);
                    param.Add("@StartTime", _StartTime);
                    param.Add("@EndTime", _EndTime);
                    param.Add("@Subject", Datas.Subject);
                    param.Add("@Content", Datas.Content);
                    param.Add("@Status", Datas.Status);
                    param.Add("@Creator", Creator);
                    param.Add("@CreatorId", CreatorId);
                    param.Add("@IsTest", Datas.IsTest);

                    conn.Execute(SpName, param, tran, commandType: CommandType.StoredProcedure);

                    int? Id = param.Get<int?>("@Id");
                    if (Id != null)
                    {
                        result = conn.Query<Announcement>("Admin_Announcement_GetById", new { Id }, tran, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    }

                    tran.Commit();
                }
            }

            return result;
        }

        public Announcement UpdateAnnouncement(Announcement Datas, string LastUpdater, int LastUpdaterId, long _StartTime, long _EndTime)
        {
            Announcement result = new Announcement();
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    string SpName = "Admin_Announcement_Upd";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@Id", Datas.Id);
                    param.Add("@SiteId", Datas.SiteId);
                    param.Add("@StartTime", _StartTime);
                    param.Add("@EndTime", _EndTime);
                    param.Add("@Subject", Datas.Subject);
                    param.Add("@Content", Datas.Content);
                    param.Add("@Status", Datas.Status);
                    param.Add("@LastUpdater", LastUpdater);
                    param.Add("@LastUpdaterId", LastUpdaterId);
                    param.Add("@IsTest", Datas.IsTest);

                    conn.Execute(SpName, param, tran, commandType: CommandType.StoredProcedure);
                    result = conn.Query<Announcement>("Admin_Announcement_GetById", new { Datas.Id }, tran, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    tran.Commit();
                }
            }

            return result;
        }

        public void DeleteAnnouncement(int Id)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Execute("Admin_Announcement_Del", new { Id }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<PersonalMsg> GetPersonalMsg(int CustId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            IEnumerable<PersonalMsg> result = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                string SpName = "Admin_PersonalMessage_Get";
                DynamicParameters props = new DynamicParameters();
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
            }

            return result;
        }

        public CustLang GetCustLang(int CustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<CustLang>("Admin_CustLang_Get", new { CustId }, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        public void DeletePersonalMsg(int Id, int CustId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Execute("Admin_PersonalMsg_Del", new { Id, CustId }, commandType: CommandType.StoredProcedure);
        }
        public bool SendMessageBatch(string personalMsg, int custId, int siteId, string sender, int batchId)
        {
            int result = 0;

            using (MySqlConnection connection = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                result = connection.Execute("Admin_Msg_BatchV2_Ins",
                    new { Sender = sender, CustId = custId, SiteId = siteId, PersonalMsg = personalMsg, BatchId = batchId },
                    commandType: CommandType.StoredProcedure);

                if (result == 1)
                {
                    transaction.Commit();
                    return true;
                }
                else
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
        public int ChangeVipLevelBatch(int siteId, int custId, string userName, int vipLevel, string memo, string Operator)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Admin_Change_VipLevel_Batch", new { SiteId = siteId, CustId = custId, UserName = userName, VipLevel = vipLevel, Memo = memo, Operator = Operator }, commandType: CommandType.StoredProcedure);
        }
        public bool NewBatchMessage(int siteId, string author, string personalMsg, ref int batchId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var p = new DynamicParameters(new { SiteId = siteId, Author = author, Message = personalMsg });
            p.Add("BatchId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = conn.Execute("Admin_BatchMessage_Ins", p, commandType: CommandType.StoredProcedure);
            batchId = p.Get<int>("BatchId");
            return result > 0;
        }
        public bool DeleteBatchMessage(int batchId)
        {
            int batchMessageCount = 0;
            int messageCount = 0;

            _logger.LogInformation($@"MessageService-DeleteBatchMessage Start.");

            using MySqlConnection conn =
                    new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var trans = conn.BeginTransaction();

            var bp = new DynamicParameters();
            bp.Add("_BatchId", batchId);

            batchMessageCount = conn.QueryFirstOrDefault<int>("Admin_BatchMessageForDelete_Get", bp, transaction: trans, commandType: CommandType.StoredProcedure);

            _logger.LogInformation($@"MessageService-DeleteBatchMessage Expect Delete Message Count={batchMessageCount}.");

            if (batchMessageCount > 0)
            {
                int affectCount = conn.QueryFirstOrDefault<int>("Admin_BatchMessageV2_Del", bp, transaction: trans, commandType: CommandType.StoredProcedure);

                if (affectCount != batchMessageCount)
                {
                    trans.Rollback();

                    _logger.LogError($@"MessageService-DeleteBatchMessage Expect Delete Batch Message Count={batchMessageCount},Actual Delete Batch Message Count={affectCount}.Transaction rollback.");

                    return false;
                }
            }

            var p = new DynamicParameters();
            p.Add("_BatchId", batchId);

            messageCount = conn.QueryFirstOrDefault<int>("Admin_MessageForDelete_Get", p, transaction: trans, commandType: CommandType.StoredProcedure);

            _logger.LogInformation($@"MessageService-DeleteBatchMessage Expect Delete Message Count={messageCount}.");

            if (messageCount > 0)
            {

                int affectCount = conn.QueryFirstOrDefault<int>("Admin_MessageByBatchId_Del", bp, transaction: trans, commandType: CommandType.StoredProcedure);

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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            _logger.LogInformation($@"MessageService-UpdateBatchMessage Start At {DateTime.UtcNow}.");
            return conn.Execute("Admin_BatchMessageUpdate_Set", new { BatchId = batchId, Message = message }, commandType: CommandType.StoredProcedure) > 0;
            _logger.LogInformation($@"MessageService-UpdateBatchMessage End At {DateTime.UtcNow}.");
        }
        public BatchMsg GetBatchMessage(int batchId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var p = new DynamicParameters(new { BatchId = batchId });
            var result = conn.Query<BatchMsg>("Admin_BatchMessage_Get", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
            return result;
        }
        public IEnumerable<BatchMsg> GetBatchMessageList(IEnumerable<int> SelectSite, long? StartDate, long? EndDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            IEnumerable<BatchMsg> result = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                string SpName = "Admin_BatchMessageList_Get";
                DynamicParameters props = new DynamicParameters();
                var _SelectSite = JsonConvert.SerializeObject(SelectSite);
                props.Add("@SelectSite", _SelectSite);
                props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@PageSize", PageSize);
                props.Add("@StartDate", StartDate);
                props.Add("@EndDate", EndDate);

                result = conn.Query<BatchMsg>(SpName, props, commandType: CommandType.StoredProcedure);
                PageNumber = props.Get<int?>("@PageNumber") ?? 0;
                TotalPages = props.Get<int?>("@TotalPages") ?? 0;
                RowCount = props.Get<int?>("@RowCount") ?? 0;
            }

            return result;
        }
        public async Task<ApiResult> SendSMSMessageToPlayerAsync(int CustId)
        {
            string apiUrl = $"{commonServiceUrl}OTPSMS/SendSMS";
            ApiResult apiResult = await courierAdapter.MyPostAsync<IEnumerable<FireBaseReturn>, dynamic>(apiUrl, new { CustId });
            return apiResult;
        }
    }
}
