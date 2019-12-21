using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class MessageService
    {
        private readonly IConfiguration configuration;

        public MessageService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public int SendMessageToPlayer(int siteId, string sender, string title, string content, int custId, string CustUserName, string lang)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("Msg_Ins", new { Type = 99, siteId = siteId, LangId = lang, RecipientCustId = custId, Recipient = CustUserName, SenderCustId = siteId, Sender = sender, Title = title, Content = content, MainId = 0 }, commandType: CommandType.StoredProcedure);
            }
        }

        public Customer GetCustInfo(int custId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<Customer>("Customer_Get", new { CustId = custId }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
