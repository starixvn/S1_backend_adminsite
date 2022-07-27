using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class ShieldService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructors

        public ShieldService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        #endregion
        
        public IEnumerable<ShieldTaggingReport> GetList(
            ShieldTaggingReportController.GetListReq request
            ,out int totalPages
            ,out int rowCount)
        {
            var tagList = Enum.GetNames(typeof(ShieldRiskTagged)).ToList();
            var props = new DynamicParameters();
            foreach (var shield in tagList)
            {
                if (request.Tags.Exists(x => x == (int) Enum.Parse(typeof(ShieldRiskTagged), shield)))
                {
                    props.Add($"_{shield}", 1 );
                }
                else
                {
                    props.Add($"_{shield}" );
                }
            }
            
            props.Add("_SiteId", request.SiteId);
            props.Add("_ShieldId", string.IsNullOrWhiteSpace(request.ShieldId) ? "" : request.ShieldId);
            props.Add("_UserName", string.IsNullOrWhiteSpace(request.UserName) ? "" : request.UserName);
            props.Add("_StartDate", request.StartDate.DateTime);
            props.Add("_EndDate", request.EndDate.DateTime);
            props.Add("_TriggeredPoint", JsonConvert.SerializeObject(request.TriggeredPoints));
            props.Add("_Platform", JsonConvert.SerializeObject(request.Platform));
            props.Add("_PageSize", Math.Abs(request.PageSize));
            props.Add("_StartIndex", (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize));
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _reportDbClient.Query<ShieldTaggingReport>("Admin_ShieldTaggingReport_Get", props).ToList();
            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;
            
            return result;
        }
        
        public IEnumerable<ShieldTaggingReportExport> GetExport(
            ShieldTaggingReportController.GetExportReq request)
        {
            var tagList = Enum.GetNames(typeof(ShieldRiskTagged)).ToList();
            var props = new DynamicParameters();
            foreach (var shield in tagList)
            {
                if (request.Tags.Exists(x => x == (int) Enum.Parse(typeof(ShieldRiskTagged), shield)))
                {
                    props.Add($"_{shield}", 1 );
                }
                else
                {
                    props.Add($"_{shield}" );
                }
            }
            
            props.Add("_SiteId", request.SiteId);
            props.Add("_ShieldId", string.IsNullOrWhiteSpace(request.ShieldId) ? "" : request.ShieldId);
            props.Add("_UserName", string.IsNullOrWhiteSpace(request.UserName) ? "" : request.UserName);
            props.Add("_StartDate", request.StartDate.DateTime);
            props.Add("_EndDate", request.EndDate.DateTime);
            props.Add("_TriggeredPoint", JsonConvert.SerializeObject(request.TriggeredPoints));
            props.Add("_Platform", JsonConvert.SerializeObject(request.Platform));
            var result = _reportDbClient.Query<ShieldTaggingReportExport>("Admin_ShieldTaggingReportExport_Get", props).ToList();
          
            
            return result;
        }
        
        public IEnumerable<ShieldAssociateAccountResult> GetAssociateAccount(
            ShieldTaggingReportController.GetAssociateAccountReq request)
        {
            var props = new DynamicParameters();
            props.Add("_ShieldId", request.ShieldId);
            var result = _reportDbClient.Query<ShieldAssociateAccountResult>("Admin_ShieldAssociateAccount_Get", props).ToList();

            return result;
        }
    }
}