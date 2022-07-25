using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class NewsServices
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public NewsServices(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public NewsData GetNewsList(string selectSite,
            DateTime startDate, DateTime endDate,
            string status, int startIndex,
            int pageSize)
        {
            var searchData = new NewsData();
            var props = new DynamicParameters();
            props.Add("_SiteIds", selectSite);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            props.Add("_Status", status);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<News>("Admin_News_Get_V4", props).ToList();

            searchData.NewsList = result;
            searchData.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("_RowCount") ?? 0;

            return searchData;
        }

        public News GetNewsById(int newsId)
        {
            return _mainDbClient.QueryFirst<News>("Admin_News_GetById_V2", new DynamicParameters(new {NewsId = newsId }));
        }

        public int? SetNews(bool isAdd, News newsDatas)
        {
            var p = new DynamicParameters();
            p.Add("SiteId", newsDatas.SiteId);
            p.Add("IsAdd", isAdd);
            p.Add("Title", newsDatas.Title);
            p.Add("Content", newsDatas.Content);
            p.Add("ImageURL", newsDatas.ImageURL);
            p.Add("Status", newsDatas.Status);
            p.Add("PostTime", newsDatas.UnixPostTime);
            p.Add("LastUpdateUser", newsDatas.LastUpdateUser);
            p.Add("Sort", newsDatas.Sort);
            p.Add("NewsId", newsDatas.NewsId, direction: ParameterDirection.InputOutput);
            _mainDbClient.Execute("Admin_News_InsUpd", p);

            return p.Get<int?>("NewsId");
        }

        public void SetStatus(int newsId, int status, string lastUpdateUser)
        {
            _mainDbClient.Execute("Admin_News_SetStatus", new DynamicParameters(new {NewsId = newsId, Status = status, LastUpdateUser = lastUpdateUser }));
        }
    }
}
