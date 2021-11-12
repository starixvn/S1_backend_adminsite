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
            var SearchResult = new AdjustmentLogReportResult();
            var props = new DynamicParameters();
            props.Add("PageNumber", para.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", para.PageSize);
            props.Add("Menu", para.Menu == "0" ? null : para.Menu);
            props.Add("StartDate", para.StartDate);
            props.Add("EndDate", para.EndDate);
            props.Add("UserName", string.IsNullOrWhiteSpace(para.User) ? null : para.User);
            var result = _reportDbClient.Query<AdjustmentLogReport>("Admin_AdjustmentInfoLog_Get", props).ToList();

            SearchResult.AdjustmentLogReport = result;
            SearchResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchResult.RowCount = props.Get<int?>("RowCount") ?? 0;
            return SearchResult;
        }
    }
}
