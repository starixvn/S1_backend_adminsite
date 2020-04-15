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

        public IEnumerable<Announcement> GetAnnouncements(int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            return messageService.GetAnnouncements(PageSize, ref PageNumber, ref TotalPages, ref RowCount);
        }

        public Announcement GetAnnouncementById(int Id)
        {
            return messageService.GetAnnouncementById(Id);
        }

        public Announcement AddAnnouncement(Announcement Datas, string Creator, int CreatorId)
        {
            return messageService.AddAnnouncement(Datas, Creator, CreatorId);
        }

        public Announcement UpdateAnnouncement(Announcement Datas, string LastUpdater, int LastUpdaterId)
        {
            return messageService.UpdateAnnouncement(Datas, LastUpdater, LastUpdaterId);
        }

        public void DeleteAnnouncement(int Id)
        {
            messageService.DeleteAnnouncement(Id);
        }
    }
}
