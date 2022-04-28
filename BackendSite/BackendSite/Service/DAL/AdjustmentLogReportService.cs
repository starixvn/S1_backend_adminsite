using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly MySqlClient _mainDbClient;
        public AdjustmentLogReportService(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
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
        
        public AdminAdjustmentLogReportResult GetAdminAdjustmentInfoLog(GetAdminAdjustmentInfoLogInput input)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", JsonConvert.SerializeObject(input.SiteIds));
            props.Add("_CurrencyIds", JsonConvert.SerializeObject(input.CurrencyIds));
            props.Add("_Menus", JsonConvert.SerializeObject(input.Menus));
            props.Add("_ActionTypes", JsonConvert.SerializeObject(input.ActionTypes));
            props.Add("_StartDate", input.StartDate.DateTime);
            props.Add("_EndDate", input.EndDate.DateTime);
            props.Add("_RefTransId", input.TransId);
            props.Add("_Operator", input.UserName);
            props.Add("_PageSize", Math.Abs(input.PageSize));
            props.Add("_StartIndex", (Math.Abs(input.PageNumber) - 1) * Math.Abs(input.PageSize));
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var result =
                _reportDbClient.Query<AdminAdjustmentLogReport>("Admin_AdminAdjustmentInfoLog_Get", props);

            var logResult = new AdminAdjustmentLogReportResult
            {
                AdminAdjustmentLogReport = result,
                TotalPages = props.Get<int?>("_TotalPages") ?? 0,
                RowCount = props.Get<int?>("_RowCount") ?? 0
            };
            return logResult;
        }
        
        public bool CreateAdminAdjustmentInfoLog(CreateAdminAdjustmentInfoLogInput input)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", input.SiteId);
            props.Add("_CurrencyId", input.CurrencyId);
            props.Add("_Menu", (int) input.MenuEnum);
            props.Add("_ActionType", (int) input.ActionType);
            props.Add("_RefTransId", input.RefTransId ?? string.Empty);
            props.Add("_BeforeDetailInfo", input.BeforeDetailInfo, DbType.String);
            props.Add("_AfterDetailInfo", input.AfterDetailInfo, DbType.String);
            props.Add("_Operator", input.Operator);
            
            var result =
                _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", props);

            return result > 0;
        }
    }
}
