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
    public class PaymentManageService
    {
        private readonly IConfiguration configuration;
        public PaymentManageService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        //public IEnumerable<PayList> GetPayInfoList()
        //{
        //    IEnumerable<PayList> payList = null;
        //    using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
        //    {
        //        conn.Open();
        //        using (var tran = conn.BeginTransaction())
        //        {
        //            string SpName = "Admin_PayList_Get";
        //            payList = conn.Query<PayList>(SpName, null, tran, commandType: CommandType.StoredProcedure);

        //            foreach (PayList payInfo in payList)
        //            {
        //                SpName = "Admin_PayChannel_Get";
        //                payInfo.ChannelList = conn.Query<PayChannel>(SpName, new { PayId = payInfo.Id }, tran, commandType: CommandType.StoredProcedure);

        //                foreach (PayChannel channel in payInfo.ChannelList)
        //                {
        //                    SpName = "Admin_PayCurrency_Get";
        //                    channel.CurrencyList = conn.Query<PayCurrency>(SpName, new { ChannelId = channel.Id }, tran, commandType: CommandType.StoredProcedure);
        //                }
        //            }

        //            tran.Commit();
        //        }
        //    }

        //    return payList;
        //}

        public IEnumerable<PayList> GetPayList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_PayList_Get";
                return conn.Query<PayList>(SpName, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<PayChannel> GetPayChannelList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_PayChannel_Get";
                return conn.Query<PayChannel>(SpName, new { PayId = 0 }, commandType: CommandType.StoredProcedure);
            }
        }

        public IEnumerable<PayCurrency> GetPayCurrencyList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_PayCurrency_Get";
                return conn.Query<PayCurrency>(SpName, new { ChannelId = 0 }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
