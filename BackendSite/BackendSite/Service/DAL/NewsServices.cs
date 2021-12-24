using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
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

        public NewsData GetNewsList(IEnumerable<int> selectSite, DateTime startDate, DateTime endDate, int pageSize, int pageNumber, int Status)
        {
            var searchData = new NewsData();
            var props = new DynamicParameters();
            props.Add("SelectSite", JsonConvert.SerializeObject(selectSite));
            props.Add("StartDate", startDate);
            props.Add("EndDate", endDate);
            props.Add("PageSize", Math.Abs(pageSize));
            props.Add("Status", Status);
            props.Add("StartIndex",(Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),DbType.Int32,ParameterDirection.Input);
            props.Add("PageNumber", Math.Abs(pageNumber),DbType.Int32, ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<News>("Admin_News_Get_V3", props).ToList();

            searchData.NewsList = result;
            searchData.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchData.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchData.RowCount = props.Get<int?>("RowCount") ?? 0;

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
