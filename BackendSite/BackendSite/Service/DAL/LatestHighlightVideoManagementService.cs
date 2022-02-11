using System;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendSite.Service.DAL
{
    public class LatestHighlightVideoManagementService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        
        public LatestHighlightVideoManagementService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }


        public LatestHighlightVideoDataResult GetLatestHighlightVideoData(GetLatestHighlightVideoDataPara para)
        {
            var result = new LatestHighlightVideoDataResult();
            var props = new DynamicParameters();

            props.Add("PageNumber", para.PageNumber,DbType.Int32,ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", para.PageSize);
            props.Add("StartDate", para.StartDate);
            props.Add("EndDate", para.EndDate);
            props.Add("LangId", para.Lang == 0 ? null : ((LangType)para.Lang).ToString()) ;
            props.Add("SelectSite", para.SelectSite != null ? para.SelectSite.Any() ? JsonConvert.SerializeObject(para.SelectSite) : null : null);

            var tempResult = _reportDbClient.Query<LatestHighlightVideoData>("Admin_LatestHighlightVideo_Get", props);

            result.LatestHighlightVideoDatas = tempResult.ToList();
            result.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            result.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            result.RowCount = props.Get<int?>("RowCount") ?? 0;

            return result;
        }
        public int SetLatestHighlightVideo(SetLatestHighlightVideoPara para,string user)
        {
            var props = new DynamicParameters();
            props.Add("Date", para.Date);
            props.Add("VideoId", para.VideoId);
            props.Add("Title", para.Title);
            props.Add("Link", para.Link);
            props.Add("Operator", user);         
            return _mainDbClient.Execute("Admin_LatestHighlightVideo_Set", props);
        }
        public void InsertLatestHighlightVideo(int siteId,string date, List<GetSiteLangData> siteLangDatas, string user)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            
            foreach (var data in siteLangDatas)
            {
                conn.Execute("Admin_LatestHighlightVideo_Int",
                    new { SiteId = siteId, Date = date, Operator = user, Title = data.Title, Link = data.Link, LangId = data.LangId }, tran,commandType: CommandType.StoredProcedure);
            }
            tran.Commit();
            
        }
    }
}
