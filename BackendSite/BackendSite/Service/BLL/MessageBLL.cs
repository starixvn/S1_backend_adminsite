using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BatchMessageSendLog;

namespace BackendSite.Service.BLL
{
    public class MessageBLL
    {
        private readonly MessageService messageService;
        private readonly CustInfoBLL custInfoBLL;

        public MessageBLL(MessageService messageService, CustInfoBLL custInfoBLL)
        {
            this.messageService = messageService;
            this.custInfoBLL = custInfoBLL;
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

        public IEnumerable<Announcement> GetAnnouncements(IEnumerable<int> selectSite, DateTimeOffset startDate, DateTimeOffset endDate, int status,int PageNumber, int PageSize, ref int TotalPages, ref int RowCount)
        {
            return messageService.GetAnnouncements(selectSite,
                startDate,
                endDate,
                status,
                (Math.Abs(PageNumber) - 1) * Math.Abs(PageSize),
                Math.Abs(PageSize),
                ref TotalPages,
                ref RowCount);
        }

        public Announcement GetAnnouncementById(int Id)
        {
            return messageService.GetAnnouncementById(Id);
        }

        public Announcement AddAnnouncement(Announcement Datas, string Creator, int CreatorId, DateTimeOffset _StartTime, DateTimeOffset _EndTime)
        {
            return messageService.AddAnnouncement(Datas, Creator, CreatorId, _StartTime, _EndTime);
        }

        public Announcement UpdateAnnouncement(Announcement Datas, string LastUpdater, int LastUpdaterId, DateTimeOffset _StartTime, DateTimeOffset _EndTime)
        {
            return messageService.UpdateAnnouncement(Datas, LastUpdater, LastUpdaterId, _StartTime, _EndTime);
        }

        public void DeleteAnnouncement(int Id)
        {
            messageService.DeleteAnnouncement(Id);
        }

        public IEnumerable<PersonalMsg> GetPersonalMsg(int CustId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            var personMsg =  messageService.GetPersonalMsg(CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
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

        public void DeletePersonalMsg(int Id, int CustId)
        {
            messageService.DeletePersonalMsg(Id, CustId);
        }

        public bool SendMessageBatch(string personalMsg, int custId, int siteId, string sender, int batchId)
        {
            return messageService.SendMessageBatch(personalMsg, custId, siteId, sender, batchId);
        }
        public int ChangeVipLevelBatch(int siteId, int custId, string userName, int vipLevel, string memo, string Operator)
        {
            return messageService.ChangeVipLevelBatch(siteId, custId, userName, vipLevel, memo, Operator);
        }
        public bool NewBatchMessage(int siteId, string author, string personalMsg, ref int batchId)
        {
            return messageService.NewBatchMessage(siteId, author, personalMsg, ref batchId);
        }
        public bool DeleteBatchMessage(int batchId)
        {
            return messageService.DeleteBatchMessage(batchId);
        }
        public bool UpdateBatchMessage(int batchId, string message)
        {
            return messageService.UpdateBatchMessage(batchId, message);
        }
        public BatchMsg GetBatchMessage(int batchId)
        {
            return messageService.GetBatchMessage(batchId);
        }
        public IEnumerable<BatchMsg> GetBatchMessageList(IEnumerable<int> SelectSite, long? StartDate, long? EndDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            var batchMessageList = messageService.GetBatchMessageList(SelectSite, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
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
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
    }
}
