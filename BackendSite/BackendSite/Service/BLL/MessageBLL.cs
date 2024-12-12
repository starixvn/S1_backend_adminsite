using System.Text.RegularExpressions;

using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BatchMessageSendLog;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendSite.Service.BLL
{
    public class MessageBLL
    {
        private readonly MessageService messageService;
        private readonly CustInfoBLL custInfoBLL;
        private readonly AdjustmentLogReportService _adjustmentLogReportService;
        private readonly ILogger<MessageBLL> _logger;
        private readonly ServerInfoService serverInfoService;
        private readonly CustInfoService _custInfoService;
        private readonly MySqlClient _mainDbClient;

        public MessageBLL(IServiceProvider provider)
        {
            this.messageService = provider.GetRequiredService<MessageService>();
            this.custInfoBLL = provider.GetRequiredService<CustInfoBLL>();
            _adjustmentLogReportService = provider.GetRequiredService<AdjustmentLogReportService>();
            _logger = provider.GetRequiredService<ILogger<MessageBLL>>();
            serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }


        public IEnumerable<Announcement> GetAnnouncements(IEnumerable<int> siteIds, DateTimeOffset startDate, DateTimeOffset endDate, IEnumerable<GeneralStatusEnum> statuses, int PageNumber, int PageSize, ref int TotalPages, ref int RowCount)
        {
            return messageService.GetAnnouncements(JsonConvert.SerializeObject(siteIds),
                startDate,
                endDate,
                JsonConvert.SerializeObject(statuses),
                (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize),
                Math.Abs(PageSize),
                ref TotalPages,
                ref RowCount);
        }

        public Announcement GetAnnouncementById(int id)
        {
            return messageService.GetAnnouncementById(id);
        }

        public Announcement AddAnnouncement(AddAnnouncementModel data, string creator, int creatorId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var result = messageService.AddAnnouncement(data, creator, creatorId, startTime, endTime);

            if (result != null)
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = creator,
                    SiteId = data.SiteId,
                    CurrencyId = 0, // 沒分
                    AfterDetailInfo = data.ToLog(),
                    ActionType = ActionType.Create,
                    MenuEnum = AdminAdjustmentLogMenuEnum.Announcement
                };

                if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
                {
                    _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
                }
            }

            return result;
        }

        public Announcement UpdateAnnouncement(UpdateAnnouncementModel data, string lastUpdater, int lastUpdaterId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var result = messageService.UpdateAnnouncement(data, lastUpdater, lastUpdaterId, startTime, endTime);
            var beforeData = GetAnnouncementById(data.Id);

            if (result != null)
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = lastUpdater,
                    SiteId = data.SiteId,
                    CurrencyId = 0, // 沒分
                    BeforeDetailInfo = beforeData.ToLog(),
                    AfterDetailInfo = data.ToLog(),
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.Announcement
                };

                if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
                {
                    _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
                }
            }

            return messageService.UpdateAnnouncement(data, lastUpdater, lastUpdaterId, startTime, endTime);
        }

        public int DeleteAnnouncement(int id, string @operator)
        {
            var result = messageService.DeleteAnnouncement(id);
            var beforeData = GetAnnouncementById(id);

            if (result >= 1)
            {
                var logParam = new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = @operator,
                    SiteId = beforeData.SiteId,
                    CurrencyId = 0, // 沒分
                    BeforeDetailInfo = beforeData.ToLog(),
                    ActionType = ActionType.Delete,
                    MenuEnum = AdminAdjustmentLogMenuEnum.Announcement
                };

                if (!_adjustmentLogReportService.CreateAdminAdjustmentInfoLog(logParam))
                {
                    _logger.LogError("CreateAdminAdjustmentInfoLog failed. params: " + JsonConvert.SerializeObject(logParam));
                }
            }
            return result;
        }

        public IEnumerable<PersonalMsg> GetPersonalMsg(int custId, int pageNumber, int pageSize,
            ref int TotalPages, ref int RowCount)
        {
            var personMsg = messageService.GetPersonalMsg(custId,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                ref TotalPages,
                ref RowCount);

            personMsg.ToList().ForEach(x =>
            {
                x.ContentRemoveTag = StripHTML(x.Content);
            });
            return personMsg;
        }
        public async Task<DataWithPage<PersonalMsg>> GetPersonalMsg(int custId, int pageNumber, int pageSize)
        {
            var personMsg = await messageService.GetPersonalMsg(custId, (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), Math.Abs(pageSize));
            personMsg.Data.ToList().ForEach(x =>
            {
                x.ContentRemoveTag = StripHTML(x.Content);
            });
            return personMsg;
        }

        public CustLang GetCustLang(int CustId)
        {
            return messageService.GetCustLang(CustId);
        }

        public async Task<bool> DeletePersonalMsg(int id, string @operator)
        {
            var personalMsg = await messageService.GetMessageById(id);
            if (personalMsg == null)
            {
                _logger.LogError("message not found");
                return false;
            }

            var customer = custInfoBLL.GetCustInfoWithCache(personalMsg.CustId);
            if (customer == null)
            {
                _logger.LogError("customer not found");
                return false;
            }

            using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();
            try
            {
                await messageService.DeletePersonalMessage(id, trans);
                await _custInfoService.InsertLogAsync(
                    customer.SiteId,
                    customer.CustId,
                    customer.CurrencyId,
                    personalMsg, null,
                    AdminAdjustmentLogMenuEnum.DeleteSMSInfo,
                    ActionType.Delete,
                    @operator, trans);
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeleteSMSInfo Fail.  SMSId:{id} Operator:{@operator}.");
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }
        }
        public int ChangeVipLevelBatch(int siteId, int currencyId ,int custId, string userName, int vipLevel, string memo, bool isPayoutReward, bool isPayoutRewardPoint, string Operator)
        {
            return messageService.ChangeVipLevelBatch(siteId, currencyId, custId, userName, vipLevel, memo, isPayoutReward, isPayoutRewardPoint, Operator);
        }
        public bool NewBatchMessage(int siteId, string author, bool isPopup, string personalMsg, ref int batchId)
        {
            return messageService.NewBatchMessage(siteId, author, personalMsg, isPopup, ref batchId);
        }
        public async Task<int> NewBatchMessage(int siteId, int currencyId, string author, bool isPopup, string personalMsg)
        {
            return await messageService.NewBatchMessage(siteId, currencyId, author, isPopup, personalMsg);
        }
        public async Task<int> DeleteBatchMessage(int siteId, int batchId, string @operator)
        {
            var batcjMsg = messageService.GetBatchMessage(batchId);
            if (batcjMsg == null) return -1;

            var isSuccess = await messageService.DeleteBatchMessage(batchId);
            if (isSuccess)
            {
                _adjustmentLogReportService.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = @operator,
                    SiteId = siteId,
                    CurrencyId = 0,
                    ActionType = ActionType.Delete,
                    MenuEnum = AdminAdjustmentLogMenuEnum.SMSGenerator,
                    BeforeDetailInfo = JsonConvert.SerializeObject(batcjMsg),
                    AfterDetailInfo = null,
                    RefTransId = batchId.ToString()
                });
            }
            return 1;
        }
        public bool UpdateBatchMessage(int batchId, bool isPopup, string message, string Operator)
        {
            return messageService.UpdateBatchMessage(batchId, isPopup, message, Operator);
        }
        public bool UpdateBatchMessageStatus(int status, int batchId)
        {
            return messageService.UpdateBatchMessageStatus(status, batchId);
        }
        public BatchMsg GetBatchMessage(int batchId)
        {
            return messageService.GetBatchMessage(batchId);
        }
        public IEnumerable<BatchMsg> GetBatchMessageList(IEnumerable<int> selectSite, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, ref int pageNumber, ref int totalPages, ref int rowCount)
        {
            var batchMessageList = messageService.GetBatchMessageList(selectSite, startDate, endDate, pageSize, ref pageNumber, ref totalPages, ref rowCount);
            batchMessageList.ToList().ForEach(x =>
            {
                x.Content = StripHTML(x.Content);
            });
            return batchMessageList;
        }
        public async Task<ApiResult> SendSMSMessageToPlayer(int CustId)
        {
            return await messageService.SendSMSMessageToPlayerAsync(CustId);
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
            return messageService.InsertBatchMessageSendLog(batchId,
                rowId,
                custId,
                userName,
                sabaId,
                resultMessage,
                updator,
                status);
        }

        public IEnumerable<BatchMessageSendLogListModel> GetBatchMessageSendLog(int batchId)
        {
            return messageService.GetBatchMessageSendLogResult(batchId);
        }

        private string StripHTML(string input)
        {
            return !string.IsNullOrEmpty(input) ? Regex.Replace(input, "<.*?>", string.Empty) : input;
        }

        public async Task<(string, CustomerWhoWaitToSend)> IsValidMember(int siteId, int? custId, string userName, string dataMessage)
        {
            var custInfosDetail = await custInfoBLL.GetCustInfoByCust(siteId, custId, userName);
            if (custInfosDetail == null)
                return ($"Not found. {dataMessage}", null);

            if (custInfosDetail.Status != CustomerStatusEnum.Running && custInfosDetail.Status != CustomerStatusEnum.Lock)
                return ($"Fail. {dataMessage} Status={custInfosDetail.Status.GetDescription()}", null);

            return ("", custInfosDetail);
        }

        public async Task<int> BatchMessageByFile(int siteId, int batchId, bool isPopup, string adminName, List<ExcelData> excelList)
        {
            try
            {
                await messageService.BeginTransaction();
                var rowId = 1;
                foreach (var data in excelList)
                {
                    await DoInsertRecipient(batchId, rowId, isPopup, siteId, data.CustId, data.CustUserName, data.SabaId, adminName);
                    rowId++;
                }
                await messageService.UpdateBatchMessage((int)SendBatchMessageStatus.Finished, batchId);
                await messageService.Commit();
                return 1;
            }
            catch (Exception ex)
            {
                await messageService.RollBack();
                _ = Task.Run(async () =>
                {
                    await messageService.DeleteBatchMessage(batchId);
                });
                _logger.LogError(ex, ex.Message);
                return 0;
            }
        }
        private async Task DoInsertRecipient(int batchId, int rawId, bool isPopup, int siteId, int? custId, string userName, string sabaId, string sender)
        {
            var dataMessage = $"Username={userName}. SabaId={sabaId}.";
            var (resultMessage, customer) = await IsValidMember(siteId, custId, userName, dataMessage);
            var tmpCustId = 0;
            if (string.IsNullOrEmpty(resultMessage))
            {
                tmpCustId = customer.CustId;
                await messageService.AddCustBatchMessage(customer.CustId, batchId, customer.UserLang, isPopup, sender);
                resultMessage = $"Success. {dataMessage} Status: {customer.Status.GetDescription()}.";
            }
            await messageService.InsertBatchMessageSendLog(batchId, rawId, tmpCustId, userName, sabaId, resultMessage, sender);
        }

        public async Task PreSendBatchMessage(ExcelData data, CustomerWhoWaitToSend customer, string personalMsg, AuthData authData, int optSite, int batchId, bool isPopup)
        {
            string resultMessage;
            var errorCode = 0;
            var dataMessage = $"CustId={customer.CustId}. Username={data.CustUserName}. SabaId={data.SabaId}.";

            if (customer == null)
            {
                resultMessage = $"Not found. {dataMessage}";
            }
            else
            {
                var customerStatus = (CustomerStatusEnum)customer.Status;
                switch (customerStatus)
                {
                    case CustomerStatusEnum.Suspend:
                    case CustomerStatusEnum.Closed:
                        resultMessage = $"Fail. {dataMessage} Status={customerStatus.GetDescription()}";
                        break;
                    case CustomerStatusEnum.Running:
                    case CustomerStatusEnum.Lock:
                        try
                        {
                            errorCode = await messageService.SendMessageBatch(personalMsg, customer.CustId, optSite, authData.UserName, batchId, isPopup);
                            resultMessage = errorCode == 1
                                ? $"Success. {dataMessage} Status: {customerStatus.GetDescription()}."
                                : $"Fail. {dataMessage} Status: {customerStatus.GetDescription()}. Insert done but affect count != 1 ";
                        }
                        catch (Exception ex)
                        {
                            resultMessage = ex.ToString();
                            errorCode = -1;
                        }
                        break;
                    default:
                        resultMessage =
                            $"Fail. {dataMessage} Status=non status";
                        break;
                }
            }

            var status = (errorCode == 1) ? 3 : 4;
            var custId = customer?.CustId ?? 0;
            if (!messageService.InsertBatchMessageSendLog(batchId, data.RowId, custId, customer.UserName, data.SabaId, resultMessage, authData.UserName, status))
            {
                _logger.LogError($@"MessageService-InsertBatchMessageSendLog Fail. " +
                                 $@"BatchId: {batchId}. CustId: {custId}. Username: {data.CustUserName}." +
                                 $@" SabaId: {data.SabaId}. Result: {resultMessage}. Status:{status}");
            }
        }
        public Task<int> SendMessageToPlayer(string sender, string title, string content, bool isPopup, string popupMessage, int custId)
        {
            var customer = custInfoBLL.GetCustInfoWithCache(custId);
            return messageService.SendMessageToPlayer(custId, customer.UserLang, isPopup, title, content, popupMessage, sender);
        }
        public int SendMessage(Message message) =>
            messageService.SendMessage(message);
        public Task<int> SendMessageAsync(Message message) =>
            messageService.SendMessageAsync(message);

        public (bool, string) CheckBatchMessage(int currency, string personalMsg)
        {
            var result = true;
            var errorMsg = "";

            if (JObject.Parse(personalMsg)["en"] == null)
            {
                errorMsg = "Missing 'en' Msg";
                return (false, errorMsg);
            }

            return (result, errorMsg);
        }
    }
}
