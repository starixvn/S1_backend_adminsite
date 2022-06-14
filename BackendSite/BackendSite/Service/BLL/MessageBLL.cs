using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BatchMessageSendLog;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class MessageBLL
    {
        private readonly MessageService messageService;
        private readonly CustInfoBLL custInfoBLL;
        private readonly ILogger<MessageBLL> _logger;

        public MessageBLL(MessageService messageService, CustInfoBLL custInfoBLL, ILogger<MessageBLL> logger)
        {
            this.messageService = messageService;
            this.custInfoBLL = custInfoBLL;
            _logger = logger;
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

        public IEnumerable<Announcement> GetAnnouncements(IEnumerable<int> selectSite, DateTimeOffset startDate, DateTimeOffset endDate, int status, int PageNumber, int PageSize, ref int TotalPages, ref int RowCount)
        {
            return messageService.GetAnnouncements(JsonConvert.SerializeObject(selectSite),
                startDate,
                endDate,
                status,
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
            return messageService.AddAnnouncement(data, creator, creatorId, startTime, endTime);
        }

        public Announcement UpdateAnnouncement(UpdateAnnouncementModel data, string lastUpdater, int lastUpdaterId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            return messageService.UpdateAnnouncement(data, lastUpdater, lastUpdaterId, startTime, endTime);
        }

        public int DeleteAnnouncement(int id)
        {
            return messageService.DeleteAnnouncement(id);
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

        public void DeletePersonalMsg(int Id)
        {
            messageService.DeletePersonalMsg(Id);
        }

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
        public bool NewBatchMessage(int siteId, string author, bool isPopup, string personalMsg,  ref int batchId)
        {
            return messageService.NewBatchMessage(siteId, author, personalMsg, isPopup, ref batchId);
        }
        public bool DeleteBatchMessage(int batchId)
        {
            return messageService.DeleteBatchMessage(batchId);
        }
        public bool UpdateBatchMessage(int batchId, bool isPopup, string message)
        {
            return messageService.UpdateBatchMessage(batchId, isPopup, message);
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

        public async Task PreSendBatchMessage(ExcelData data, CustomerWhoWaitToSend customer, string personalMsg, AuthData authData, int optSite, int batchId, bool isPopup)
        {
            string resultMessage;
            var errorCode = 0;
            var dataMessage = $"CustId={data.CustId}. Username={data.CustUserName}. SabaId={data.SabaId}.";
            
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

            var status= (errorCode == 1) ? 3 : 4;
            var custId = customer?.CustId ?? 0;
            if (!messageService.InsertBatchMessageSendLog(batchId, data.RowId, custId, data.CustUserName, data.SabaId, resultMessage, authData.UserName, status))
            {
                _logger.LogError($@"MessageService-InsertBatchMessageSendLog Fail. " +
                                 $@"BatchId: {batchId}. CustId: {custId}. Username: {data.CustUserName}." +
                                 $@" SabaId: {data.SabaId}. Result: {resultMessage}. Status:{status}");
            }
        }
    }
}
