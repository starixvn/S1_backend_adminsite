using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public IEnumerable<Announcement> GetAnnouncements(int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            return messageService.GetAnnouncements(PageSize, ref PageNumber, ref TotalPages, ref RowCount);
        }

        public Announcement GetAnnouncementById(int Id)
        {
            return messageService.GetAnnouncementById(Id);
        }

        public Announcement AddAnnouncement(Announcement Datas, string Creator, int CreatorId, long _StartTime, long _EndTime)
        {
            return messageService.AddAnnouncement(Datas, Creator, CreatorId, _StartTime, _EndTime);
        }

        public Announcement UpdateAnnouncement(Announcement Datas, string LastUpdater, int LastUpdaterId, long _StartTime, long _EndTime)
        {
            return messageService.UpdateAnnouncement(Datas, LastUpdater, LastUpdaterId, _StartTime, _EndTime);
        }

        public void DeleteAnnouncement(int Id)
        {
            messageService.DeleteAnnouncement(Id);
        }

        public IEnumerable<PersonalMsg> GetPersonalMsg(int CustId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            return messageService.GetPersonalMsg(CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
        }

        public CustLang GetCustLang(int CustId)
        {
            return messageService.GetCustLang(CustId);
        }

        public void DeletePersonalMsg(int Id, int CustId)
        {
            messageService.DeletePersonalMsg(Id, CustId);
        }

        public int SendMessageBatch(string personalMsg, int custId, int siteId, string sender ,int batchId)
        {
            return messageService.SendMessageBatch(personalMsg, custId, siteId, sender, batchId);
        }
        public int ChangeVipLevelBatch(int siteId, int custId, string userName, int vipLevel, string memo, string Operator)
        {
            return messageService.ChangeVipLevelBatch(siteId, custId, userName, vipLevel, memo, Operator);
        }
        public bool NewBatchMessage(string author, string personalMsg, ref int batchId)
        {
            return messageService.NewBatchMessage( author, personalMsg,ref batchId);
        }
        public bool DeleteBatchMessage(int batchId)
        {
            return messageService.DeleteBatchMessage( batchId);
        }
        public bool UpdateBatchMessage(int batchId, string message)
        {
            return messageService.UpdateBatchMessage( batchId, message);
        }
        public string GetBatchMessage(int batchId)
        {
            return messageService.GetBatchMessage( batchId);
        }
        public IEnumerable<BatchMsg> GetBatchMessageList(long? StartDate, long? EndDate, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            return messageService.GetBatchMessageList(StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages,  ref RowCount);
        }



    }
}
