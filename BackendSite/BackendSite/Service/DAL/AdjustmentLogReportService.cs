using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            props.Add("PageSize", Math.Abs(para.PageSize));
            props.Add("Menu", para.Menu == "0" ? null : para.Menu);
            props.Add("StartDate", para.StartDate.DateTime);
            props.Add("EndDate", para.EndDate.DateTime);
            props.Add("UserName", string.IsNullOrWhiteSpace(para.User) ? null : para.User);
            props.Add("PageNumber", Math.Abs(para.PageNumber), DbType.Int32, ParameterDirection.InputOutput);
            props.Add("StartIndex", (Math.Abs(para.PageNumber) - 1) * Math.Abs(para.PageSize),DbType.Int32,ParameterDirection.Input);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<AdjustmentLogReport>("Admin_AdjustmentInfoLog_Get_V2", props).ToList();

            searchResult.AdjustmentLogReport = result;
            searchResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            searchResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            searchResult.RowCount = props.Get<int?>("RowCount") ?? 0;
            return searchResult;
        }
    }
}
