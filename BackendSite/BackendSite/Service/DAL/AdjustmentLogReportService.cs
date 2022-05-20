using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BackendSite.Service.DAL
{
    public class AdjustmentLogReportService
    {
        private readonly MySqlClient _reportDbClient;
        public AdjustmentLogReportService(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public List<AdjustmentInfoLogMenu> GetAdjustmentInfoLogMenu()
        {
            var props = new DynamicParameters();
            return _reportDbClient.Query<AdjustmentInfoLogMenu>("Admin_AdjustmentInfoLog_Menu_Get", props).ToList();
        }
        public AdjustmentLogReportResult GetAdjustmentInfoLog(GetAdjustmentInfoLogPara para)
        {
            var searchResult = new AdjustmentLogReportResult();
            var props = new DynamicParameters();
            props.Add("_Menus", JsonConvert.SerializeObject(para.Menus));
            props.Add("_StartDate", para.StartDate.DateTime);
            props.Add("_EndDate", para.EndDate.DateTime);
            props.Add("_UserName", string.IsNullOrWhiteSpace(para.User) ? null : para.User);
            props.Add("_PageSize", Math.Abs(para.PageSize));
            props.Add("_StartIndex", (Math.Abs(para.PageNumber) - 1) * Math.Abs(para.PageSize));
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<AdjustmentLogReport>("Admin_AdjustmentInfoLog_Get_V3", props).ToList();

            searchResult.AdjustmentLogReport = result;
            searchResult.TotalPages = props.Get<int?>("_TotalPages") ?? 0;
            searchResult.RowCount = props.Get<int?>("_RowCount") ?? 0;
            return searchResult;
        }
    }
}
