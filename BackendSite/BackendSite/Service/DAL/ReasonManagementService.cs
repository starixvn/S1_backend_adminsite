using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class ReasonManagementService
    {
        private readonly IConfiguration configuration;

        public ReasonManagementService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<ReasonManagementFuncKey> GetFuncKey()
        {
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<ReasonManagementFuncKey>("Admin_BankFinancialReason_FuncKey_Get", commandType: CommandType.StoredProcedure);
        }

        public GetReasonManagementDataResult GetReasonManagementData(GetReasonManagementDataPara para)
        {
            GetReasonManagementDataResult SearchResult = new GetReasonManagementDataResult();
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            DynamicParameters props = new DynamicParameters();
            props.Add("PageNumber", para.PageNumber, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
            props.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("PageSize", para.PageSize);
            props.Add("FuncKey", para.FuncKey == "0" ? null: para.FuncKey);

            var result = conn.Query<ReasonManagementData>("Admin_BankFinancialReason_Get", props, commandType: CommandType.StoredProcedure);

            SearchResult.ReasonManagementData = result.ToList();
            SearchResult.PageNumber = props.Get<int?>("PageNumber") ?? 0;
            SearchResult.TotalPages = props.Get<int?>("TotalPages") ?? 0;
            SearchResult.RowCount = props.Get<int?>("RowCount") ?? 0;

            return SearchResult;
        }

        public int SetReasonManagement(SetReasonManagementPara para)
        {
            DynamicParameters props = new DynamicParameters();
            using MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));

            //props.Add("CustIds", CustIds);
            //props.Add("CalculateMonth", YearMonth);
            //props.Add("Status", ChangeStatus);
            //props.Add("LastUpdateUser", username);

            return conn.Execute("Admin_VipUpgradeReview_Set", props, commandType: CommandType.StoredProcedure);
        }
    }
}
