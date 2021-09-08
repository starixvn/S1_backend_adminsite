using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BackendSite.Service.DAL
{
    public class LatestHighlightVideoManagementService
    {
        private readonly IConfiguration configuration;

        public LatestHighlightVideoManagementService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public LatestHighlightVideoDataResult GetLatestHighlightVideoData(GetLatestHighlightVideoDataPara para)
        {
            var result = new LatestHighlightVideoDataResult();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();

            props.Add("PageNumber", para.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", para.PageSize);
            props.Add("StartDate", para.StartDate);
            props.Add("EndDate", para.EndDate);
            props.Add("LangId", para.Lang == 0 ? null : ((LangType)para.Lang).ToString()) ;
            props.Add("SelectSite", para.SelectSite != null ? para.SelectSite.Any() ? JsonConvert.SerializeObject(para.SelectSite) : null : null);

            var tempresult = conn.Query<LatestHighlightVideoData>("Admin_LatestHighlightVideo_Get", props, commandType: CommandType.StoredProcedure);

            result.LatestHighlightVideoDatas = tempresult.ToList();
            result.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            result.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            result.RowCount = props.Get<int?>("RowCount") ?? 0;

            return result;
        }
        public int SetLatestHighlightVideo(SetLatestHighlightVideoPara para,string User)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            var props = new DynamicParameters();
            props.Add("Date", para.Date);
            props.Add("VideoId", para.VideoId);
            props.Add("Title", para.Title);
            props.Add("Link", para.Link);
            props.Add("Operator", User);         
            return conn.Execute("Admin_LatestHighlightVideo_Set", props, commandType: CommandType.StoredProcedure);
        }
        public void InsertLatestHighlightVideo(int SiteId,string Date, List<GetSiteLangData> SiteLangDatas, string User)
        {
            var result = new LatestHighlightVideoDataResult();
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    var props = new DynamicParameters();

                    foreach (var data in SiteLangDatas)
                    {
                        conn.Execute("Admin_LatestHighlightVideo_Int",
                            new { SiteId = SiteId, Date = Date, Operator = User, Title = data.Title, Link = data.Link, LangId = data.LangId }, tran,commandType: CommandType.StoredProcedure);
                    }
                    tran.Commit();
                }
            };
            
        }
    }
}
