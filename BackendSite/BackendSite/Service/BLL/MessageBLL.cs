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

        public int SendMessageToPlayer(string sender, string title, string content, int custId)
        {
            var customer = custInfoBLL.GetCustInfoWithCache(custId);
            return messageService.SendMessageToPlayer(customer.SiteId, sender, title, content, custId, customer.UserName, customer.UserLang);
        }

        public int SendMessageToPlayer(string sender, string title, string content, int custId, int SenderCustId)
        {
            var customer = custInfoBLL.GetCustInfoWithCache(custId);
            return messageService.SendMessageToPlayer(customer.SiteId, sender, title, content, custId, customer.UserName, customer.UserLang, SenderCustId);
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

        public CustLang GetCustLang(int CustId)
        {
            return messageService.GetCustLang(CustId);
        }

        public async Task<bool> DeletePersonalMsg(int id, bool isBatch, string @operator)
        {
            var msgBeforeInfo = await GetMessageById(id, isBatch);
            if (!msgBeforeInfo.Any()) return false;
            var custSimpleInfo = await custInfoBLL.GetCustId(msgBeforeInfo.First().CustId, null, null);
            await using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            await using var trans = await conn.BeginTransactionAsync();

            if (!messageService.DeletePersonalMsg(id, isBatch))
            {
                _logger.LogError($"DeleteSMSInfo: Delete SMSInfo Fail. SMSId:{id} IsBatch:{isBatch} Operator:{@operator}.");
                trans.Rollback();
                return false;
            }

            if (!await _custInfoService.InsertLogAsync(custSimpleInfo.SiteId, custSimpleInfo.CustId, custSimpleInfo.CurrencyId, msgBeforeInfo.First(), null,
                AdminAdjustmentLogMenuEnum.DeleteSMSInfo, ActionType.Delete, @operator, trans))
            {
                _logger.LogError($"DeleteSMSInfo: Insert Operate Log Fail.  SMSId:{id} IsBatch:{isBatch} Operator:{@operator}.");
                trans.Rollback();
                return false;
            }

            trans.Commit();
            return true;
        }

        public async Task<IEnumerable<PersonalMsg>> GetMessageById(int id, bool isBatch) => await messageService.GetMessageById(id, isBatch);

        public bool SendMessageBatch(string personalMsg, int custId, int siteId, string sender, int batchId)
        {
            return messageService.SendMessageBatch(personalMsg, custId, siteId, sender, batchId);
        }
        public async Task<int> SendMessageBatch(string personalMsg, int custId, int siteId, string sender, int batchId, bool isPopup)
        {
            return await messageService.SendMessageBatch(personalMsg, custId, siteId, sender, batchId, isPopup);
        }
        public int ChangeVipLevelBatch(int siteId, int custId, string userName, int vipLevel, string memo, string Operator)
        {
            return messageService.ChangeVipLevelBatch(siteId, custId, userName, vipLevel, memo, Operator);
        }
        public bool NewBatchMessage(int siteId, string author, bool isPopup, string personalMsg, ref int batchId)
        {
            return messageService.NewBatchMessage(siteId, author, personalMsg, isPopup, ref batchId);
        }
        public async Task<int> NewBatchMessage(int siteId, int currencyId, string author, bool isPopup, string personalMsg)
        {
            return await messageService.NewBatchMessage(siteId, currencyId, author, isPopup, personalMsg);
        }
        public bool DeleteBatchMessage(int batchId)
        {
            return messageService.DeleteBatchMessage(batchId);
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
                return batchId;
            }
            catch (Exception ex)
            {
                await messageService.RollBack();
                _logger.LogError(ex, ex.Message);
                return 0;
            }
        }
        private async Task DoInsertRecipient(int batchId, int rawId, bool isPopup, int siteId, int? custId, string userName, string sabaId, string adminName)
        {
            var dataMessage = $"Username={userName}. SabaId={sabaId}.";
            var (resultMessage, customer) = await IsValidMember(siteId, custId, userName, dataMessage);
            var tmpCustId = 0;
            if (string.IsNullOrEmpty(resultMessage))
            {
                tmpCustId = customer.CustId;
                await messageService.AddCustBatchMessage(customer.CustId, batchId, customer.UserLang, isPopup, "", "");
                resultMessage = $"Success. {dataMessage} Status: {customer.Status.GetDescription()}.";
            }
            await messageService.InsertBatchMessageSendLog(batchId, rawId, tmpCustId, userName, sabaId, resultMessage, adminName);
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

        public (bool, string) CheckBatchMessage(int currency, string personalMsg)
        {
            var result = true;
            var errorMsg = "";

            switch (currency)
            {
                case (int)CurrencyEnum.MYR:
                    if (JObject.Parse(personalMsg)["cs"] == null ||
                        JObject.Parse(personalMsg)["my"] == null)
                    {
                        result = false;
                        errorMsg = "Missing 'cs' or 'my' Msg";
                    }
                    break;

                case (int)CurrencyEnum.USD:
                case (int)CurrencyEnum.CNY:
                    if (JObject.Parse(personalMsg)["cs"] == null)
                    {
                        result = false;
                        errorMsg = "Missing 'cs' Msg";
                    }
                    break;

                case (int)CurrencyEnum.THB:
                    if (JObject.Parse(personalMsg)["th"] == null ||
                        JObject.Parse(personalMsg)["en"] == null)
                    {
                        errorMsg = "Missing 'th' or 'en' Msg";
                        result = false;
                    }
                    break;


                case (int)CurrencyEnum.IDR:
                    if (JObject.Parse(personalMsg)["id"] == null ||
                        JObject.Parse(personalMsg)["en"] == null)
                    {
                        errorMsg = "Missing 'id' or 'en' Msg";
                        result = false;
                    }
                    break;

                case (int)CurrencyEnum.INR:
                    if (JObject.Parse(personalMsg)["hi"] == null ||
                        JObject.Parse(personalMsg)["en"] == null)
                    {
                        errorMsg = "Missing 'hi' or 'en' Msg";
                        result = false;
                    }
                    break;

                case (int)CurrencyEnum.BDT:
                    if (JObject.Parse(personalMsg)["bd"] == null)
                    {
                        errorMsg = "Missing 'bd' Msg";
                        result = false;
                    }
                    break;
            }

            return (result, errorMsg);
        }
    }
}
