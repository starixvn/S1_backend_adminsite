using System.Data;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BatchMessageSendLog;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class MessageService
    {
        private readonly string _commonServiceUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly ILogger<MessageService> _logger;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private MySqlConnection _connection;
        private MySqlTransaction _transaction;

        public MessageService(IServiceProvider provider)
        {
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _commonServiceUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").CommonService;
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _logger = provider.GetRequiredService<ILogger<MessageService>>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }


        public IEnumerable<Announcement> GetAnnouncements(string siteIds,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            string statuses,
            int startIndex,
            int pageSize,
            ref int totalPages,
            ref int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);
            props.Add("_StartDate", startDate.DateTime, DbType.DateTime);
            props.Add("_EndDate", endDate.DateTime, DbType.DateTime);
            props.Add("_Statuses", statuses);
            props.Add("_StartIndex", startIndex, DbType.Int32);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<Announcement>("Admin_Announcement_Get_V3", props);
            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public Announcement GetAnnouncementById(int id)
        {
            return _mainDbClient.QueryFirst<Announcement>("Admin_Announcement_GetById_V2", new DynamicParameters(new { Id = id }));
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
            param.Add("@Seq", data.Seq);

            _mainDbClient.Execute("Admin_Announcement_Ins_V3", param);

            var id = param.Get<int?>("@Id");
            if (id != null)
            {
                result = _mainDbClient.QueryFirst<Announcement>("Admin_Announcement_GetById_V2", new DynamicParameters(new { Id = id }));
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
            param.Add("@Seq", data.Seq);

            _mainDbClient.Execute("Admin_Announcement_Upd_V3", param);
            var result = _mainDbClient.QueryFirst<Announcement>("Admin_Announcement_GetById_V2", new DynamicParameters(new { data.Id }));

            return result;
        }

        public int DeleteAnnouncement(int id)
        {
            return _mainDbClient.Execute("Admin_Announcement_Del", new DynamicParameters(new { Id = id }));
        }
        public Task<int> SendMessageToPlayer(int custId, string langId, bool isPopup, string title, string content, string popupMessage, string sender)
        {
            return _mainDbClient.ExecuteAsync(
                "Admin_CustBatchMessage_Ins_V2",
                new { _CustId = custId, _BatchId = 0, _LangId = langId, _IsPopup = isPopup, _Title = title, _Content = content, _PopupMessage = popupMessage, _Sender = sender });
        }
        public IEnumerable<PersonalMsg> GetPersonalMsg(int custId, int startIndex, int PageSize,
            ref int TotalPages, ref int RowCount)
        {
            IEnumerable<PersonalMsg> result = null;
            var SpName = "Admin_PersonalMessage_Get_V3";
            var props = new DynamicParameters();
            props.Add("_CustId", custId, dbType: DbType.Int32);
            props.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            props.Add("_PageSize", PageSize, dbType: DbType.Int32);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            result = _reportDbClient.Query<PersonalMsg>(SpName, props);
            TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            RowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }
        public async Task<DataWithPage<PersonalMsg>> GetPersonalMsg(int custId, int startIndex, int PageSize)
        {
            var props = new DynamicParameters();
            props.Add("_CustId", custId, dbType: DbType.Int32);
            props.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            props.Add("_PageSize", PageSize, dbType: DbType.Int32);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = await _reportDbClient.QueryAsync<PersonalMsg>("Admin_PersonalMessage_Get_V5", props);
            return new DataWithPage<PersonalMsg>()
            {
                Data = result,
                PageNumber = startIndex,
                TotalPages = props.Get<int?>("_TotalPages") ?? 0,
                RowCount = props.Get<int?>("_RowCount") ?? 0
            };
        }
        internal async Task<PersonalMsg> GetMessageById(int id) =>
            await _reportDbClient.QueryFirstAsync<PersonalMsg>("Admin_PersonalMessageById_Get", new { _Id = id });

        public async Task<int> DeletePersonalMessage(int id, MySqlTransaction transaction) =>
             await _mainDbClient.ExecuteAsync("Admin_PersonalMsg_Del_V3", transaction, new { _Id = id });

        public CustLang GetCustLang(int CustId)
        {
            return _reportDbClient.Query<CustLang>("Admin_CustLang_Get", new { CustId }).FirstOrDefault();
        }

        public bool SendMessageBatch(string personalMsg, int custId, int siteId, string sender, int batchId)
        {
            try
            {
                using var connection = _mainDbClient.GetMySqlConnection();
                connection.Open();
                using var trans = connection.BeginTransaction();
                var result = _mainDbClient.Execute("Admin_Msg_BatchV2_Ins", trans, new DynamicParameters(new { Sender = sender, CustId = custId, SiteId = siteId, PersonalMsg = personalMsg, BatchId = batchId }));

                if (result == 1)
                {
                    _mainDbClient.CommitAndDisposeConnection(connection, trans);
                    return true;
                }
                _mainDbClient.RollbackAndDisposeConnection(connection, trans);
                return false;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public async Task<int> SendMessageBatch(string personalMsg, int custId, int siteId, string sender, int batchId, bool isPopup)
        {
            var p = new DynamicParameters();
            p.Add("_Sender", sender);
            p.Add("_CustId", custId);
            p.Add("_SiteId", siteId);
            p.Add("PersonalMsg", personalMsg);
            p.Add("_BatchId", batchId);
            p.Add("_IsPopup", isPopup);
            return await _mainDbClient.ExecuteAsync("Admin_Msg_Batch_Ins_V3", p);
        }
        public int ChangeVipLevelBatch(int siteId, int currencyId ,int custId, string userName, int vipLevel, string memo, bool isPayoutReward, bool isPayoutRewardPoint, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteId);
            props.Add("_CurrencyId", currencyId);
            props.Add("_CustId", custId);
            props.Add("_UserName", userName);
            props.Add("_VipLevel", vipLevel);
            props.Add("_Memo", memo);
            props.Add("_IsPayoutReward", isPayoutReward);
            props.Add("_IsPayoutRewardPoint", isPayoutRewardPoint);
            props.Add("_Operator", Operator);
            props.Add("_Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
            _ = _mainDbClient.Execute("Admin_Change_VipLevel_Batch_V2", props);
            
            var result = props.Get<int?>("_Result") ?? -1;

            return result;
        }
        public bool NewBatchMessage(int siteId, string author, string personalMsg, bool isPopup, ref int batchId)
        {
            var p = new DynamicParameters(new { _SiteId = siteId, _Author = author, _IsPopup = isPopup, _Message = personalMsg });
            p.Add("BatchId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _mainDbClient.Execute("Admin_BatchMessage_Ins_V2", p);
            batchId = p.Get<int>("BatchId");
            return result > 0;
        }
        public async Task<bool> DeleteBatchMessage(int batchId)
        {
            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();

            try
            {
                var p = new { _BatchId = batchId };
                await _mainDbClient.ExecuteAsync("Admin_BatchMessageV2_Del", trans, p);
                await _mainDbClient.ExecuteAsync("Admin_CustBatchMessageByBatchId_Del", trans, p);
                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "delete batch message failed");
                await trans.RollbackAsync();
                return false;
            }
        }
        public bool UpdateBatchMessage(int batchId, bool isPopup, string message, string Operator)
        {
            var props = new DynamicParameters();
            props.Add("_BatchId", batchId);
            props.Add("_IsPopup", isPopup);
            props.Add("_Message", message);
            props.Add("_Operator", Operator);
            return _mainDbClient.Execute("Admin_BatchMessageUpdate_Set_V3", props) >= 0;
        }

        public bool UpdateBatchMessageStatus(int status, int batchId)
        {
            var props = new DynamicParameters();
            props.Add("_Status", status);
            props.Add("_BatchId", batchId);
            return _mainDbClient.Execute("Admin_BatchMessageUpdateStatus_Set", props) > 0;
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
            props.Add("@StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), DbType.Int32);
            props.Add("@PageNumber", Math.Abs(pageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("@PageSize", Math.Abs(pageSize));
            props.Add("@StartDate", startDate.DateTime);
            props.Add("@EndDate", endDate.DateTime);

            var result = _mainDbClient.Query<BatchMsg>("Admin_BatchMessageList_Get_V2", props);
            pageNumber = props.Get<int?>("@PageNumber") ?? 0;
            totalPages = props.Get<int?>("@TotalPages") ?? 0;
            rowCount = props.Get<int?>("@RowCount") ?? 0;

            return result;
        }
        public async Task<ApiResult> SendSMSMessageToPlayerAsync(int CustId)
        {
            var apiUrl = $"{_commonServiceUrl}OTPSMS/SendSMS";
            ApiResult apiResult = await _courierAdapter.MyPostAsync<ApiResult, dynamic>(apiUrl, new { CustId });
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

        internal async Task<IEnumerable<PersonalMsg>> GetMessageById(int id, bool isBatch)
        {
            return await _reportDbClient.QueryAsync<PersonalMsg>("Admin_PersonalMessage_GetById", new DynamicParameters(new { _Id = id, _IsBatch = isBatch }));
        }


        public async Task<int> NewBatchMessage(int siteId, int currencyId, string author, bool isPopup, string personalMsg)
        {
            var p = new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId, _Author = author, _IsPopup = isPopup, _Message = personalMsg });
            p.Add("BatchId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            await _mainDbClient.ExecuteAsync("Admin_BatchMessage_Ins_V3", p);
            var batchId = p.Get<int?>("BatchId") ?? 0;
            return batchId;
        }
        public Task<int> AddCustBatchMessage(int custId, int batchId, string langId, bool isPopup, string sender)
        {
            return _mainDbClient.ExecuteAsync(
                "Admin_CustBatchMessage_Ins_V2",
                _transaction,
                new { _CustId = custId, _BatchId = batchId, _LangId = langId, _IsPopup = isPopup, _Title = "", _Content = "", _PopupMessage = "", _Sender = sender });
        }
        public async Task<int> InsertBatchMessageSendLog(int batchId, int rowId, int custId, string userName, string sabaId, string resultMessage, string updator)
        {
            var props = new DynamicParameters();
            props.Add("_BatchId", batchId);
            props.Add("_Seq", rowId);
            props.Add("_CustId", custId);
            props.Add("_UserName", userName);
            props.Add("_SabaId", sabaId);
            props.Add("_Result", resultMessage);
            props.Add("_Creator", updator);
            return await _mainDbClient.ExecuteAsync("Admin_BatchMessageSendLog_Ins_V2", _transaction, props);
        }
        public async Task<int> UpdateBatchMessage(int status, int batchId)
            => await _mainDbClient.ExecuteAsync("Admin_BatchMessageUpdateStatus_Set",
                _transaction,
                new { _BatchId = batchId, _Status = status });

        public async Task BeginTransaction()
        {
            _connection = _mainDbClient.GetMySqlConnection();
            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }
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
        public int SendMessageToPlayer(string sender, int custId, string lang, string title, string content)
        {
            return _mainDbClient.Execute(
                "Admin_CustBatchMessage_Ins",
                new { _CustId = custId, _BatchId = 0, _LangId = lang, _IsPopup = 0, _Title = title, _Content = content, _Sender = sender });
        }
        public int SendMessage(Message message) =>
            _mainDbClient.Execute("Msg_Ins_V4", new DynamicParameters(message));

        public async Task<int> SendMessageAsync(Message message) =>
            await _mainDbClient.ExecuteAsync("Msg_Ins_V4", new DynamicParameters(message));
    }
}
