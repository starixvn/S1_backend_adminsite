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
    public class NewsServices
    {
        private readonly IConfiguration configuration;
        public NewsServices(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<News> GetNewsList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<News>("Admin_News_Get", commandType: CommandType.StoredProcedure);
            }
        }

        public News GetNewsById(int NewsId)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.QueryFirstOrDefault<News>("Admin_News_GetById", new { NewsId }, commandType: CommandType.StoredProcedure);
            }
        }

        public int? SetNews(bool IsAdd, News newsDatas)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            var p = new DynamicParameters();
            p.Add("SiteId", newsDatas.SiteId);
            p.Add("IsAdd", IsAdd);
            p.Add("Title", newsDatas.Title);
            p.Add("Content", newsDatas.Content);
            p.Add("ImageURL", newsDatas.ImageURL);
            p.Add("Status", newsDatas.Status);
            p.Add("PostTime", newsDatas.UnixPostTime);
            p.Add("LastUpdateUser", newsDatas.LastUpdateUser);
            p.Add("Sort", newsDatas.Sort);
            p.Add("NewsId", newsDatas.NewsId, direction: ParameterDirection.InputOutput);

            conn.Execute("Admin_News_InsUpd", p, commandType: CommandType.StoredProcedure);

            return p.Get<int?>("NewsId");
        }

        public void SetStatus(int NewsId, int Status, string LastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Execute("Admin_News_SetStatus", new { NewsId, Status, LastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
