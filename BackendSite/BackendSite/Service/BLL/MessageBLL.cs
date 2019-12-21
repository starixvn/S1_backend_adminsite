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
        private readonly ServerInfoService serverInfoService;

        public MessageBLL(MessageService messageService, ServerInfoService serverInfoService)
        {
            this.messageService = messageService;
            this.serverInfoService = serverInfoService;
        }

        public int SendMessageToPlayer(string sender, string title, string content, int custId)
        {
            var serverInfo = serverInfoService.GetModel();
            Customer customer = messageService.GetCustInfo(custId);
            return messageService.SendMessageToPlayer(serverInfo.SiteId, sender, title, content, custId, customer.UserName, serverInfo.DefLang);
        }
    }
}
