using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BackendSite.Service.DAL
{
    public class NewsServices
    {
        private readonly IConfiguration configuration;
        public NewsServices(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public NewsData GetNewsList(IEnumerable<int> SelectSite, long StartDate, long EndDate, int PageSize, int PageNumber, int Status)
        {
            var SearchData = new NewsData();
            var props = new DynamicParameters();
            props.Add("SelectSite", JsonConvert.SerializeObject(SelectSite));
            props.Add("StartDate", StartDate);
            props.Add("EndDate", EndDate);
            props.Add("PageSize", PageSize);
            props.Add("Status", Status);
            props.Add("PageNumber", PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));

            var result = conn.Query<News>("Admin_NewsV2_Get", props, commandType: CommandType.StoredProcedure).ToList();

            SearchData.NewsList = result;
            SearchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchData.RowCount = props.Get<int?>("RowCount") ?? 0;

            return SearchData;
        }

        public News GetNewsById(int NewsId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
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
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Execute("Admin_News_SetStatus", new { NewsId, Status, LastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
