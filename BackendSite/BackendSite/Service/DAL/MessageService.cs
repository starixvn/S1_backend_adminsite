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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("Msg_Ins", new { Type = 99, siteId = siteId, LangId = lang, RecipientCustId = custId, Recipient = CustUserName, SenderCustId = siteId, Sender = sender, Title = title, Content = content, MainId = 0 }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Announcement> GetAnnouncements(int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            IEnumerable<Announcement> result = null;
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SpName = "Admin_Announcement_Get";
                DynamicParameters props = new DynamicParameters();
                props.Add("@PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                props.Add("@TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
                props.Add("@StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);

                props.Add("@PageSize", PageSize);

                result = conn.Query<Announcement>(SpName, props, commandType: CommandType.StoredProcedure);
                PageNumber = props.Get<int?>("@PageNumber") ?? 0;
                TotalPages = props.Get<int?>("@TotalPages") ?? 0;
                RowCount = props.Get<int?>("@RowCount") ?? 0;
            }

            return result;
        }

        public Announcement GetAnnouncementById(int Id)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<Announcement>("Admin_Announcement_GetById", new { Id }, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        public Announcement AddAnnouncement(Announcement Datas, string Creator, int CreatorId, long _StartTime, long _EndTime)
        {
            Announcement result = new Announcement();
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    string SpName = "Admin_Announcement_Ins";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    param.Add("@SiteId", 0);
                    param.Add("@StartTime", _StartTime);
                    param.Add("@EndTime", _EndTime);
                    param.Add("@Subject", Datas.Subject);
                    param.Add("@Content", Datas.Content);
                    param.Add("@Status", Datas.Status);
                    param.Add("@Creator", Creator);
                    param.Add("@CreatorId", CreatorId);
                    param.Add("@IsTest", Datas.IsTest);
                    
                    conn.Execute(SpName, param, tran, commandType: CommandType.StoredProcedure);

                    int? Id = param.Get<int?>("@Id");
                    if (Id != null)
                    {
                        result = conn.Query<Announcement>("Admin_Announcement_GetById", new { Id }, tran, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    }

                    tran.Commit();
                }
            }

            return result;
        }

        public Announcement UpdateAnnouncement(Announcement Datas, string LastUpdater, int LastUpdaterId, long _StartTime, long _EndTime)
        {
            Announcement result = new Announcement();
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    string SpName = "Admin_Announcement_Upd";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@Id", Datas.Id);
                    param.Add("@SiteId", 0);
                    param.Add("@StartTime", _StartTime);
                    param.Add("@EndTime", _EndTime);
                    param.Add("@Subject", Datas.Subject);
                    param.Add("@Content", Datas.Content);
                    param.Add("@Status", Datas.Status);
                    param.Add("@LastUpdater", LastUpdater);
                    param.Add("@LastUpdaterId", LastUpdaterId);
                    param.Add("@IsTest", Datas.IsTest);

                    conn.Execute(SpName, param, tran, commandType: CommandType.StoredProcedure);
                    result = conn.Query<Announcement>("Admin_Announcement_GetById", new { Datas.Id }, tran, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    tran.Commit();
                }
            }

            return result;
        }

        public void DeleteAnnouncement(int Id)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Execute("Admin_Announcement_Del", new { Id }, commandType: CommandType.StoredProcedure);
        }
    }
}
