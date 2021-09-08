using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<ReasonManagementFuncKey>("Admin_BankFinancialReason_FuncKey_Get", commandType: CommandType.StoredProcedure);
        }

        public GetReasonManagementDataResult GetReasonManagementData(GetReasonManagementDataPara para)
        {
            var SearchResult = new GetReasonManagementDataResult();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            var props = new DynamicParameters();
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

        public int SetReasonManagement(SetReasonManagementPara para,int UserId, DateTime now)
        {
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));

            props.Add("Id", para.Id);
            props.Add("Description", para.Description);
            props.Add("UpdateTime", now);
            props.Add("Updator", UserId);

            return conn.Execute("Admin_BankFinancialReason_Upd", props, commandType: CommandType.StoredProcedure);
        }
        public int InsertReasonManagement(InsertReasonManagementPara para, int UserId, DateTime now)
        {
            var props = new DynamicParameters();
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));

            props.Add("FuncKey", para.FuncKey);
            props.Add("Description", para.Description);
            props.Add("UpdateTime", now);
            props.Add("Updator", UserId);
            props.Add("CreateTime", now);
            props.Add("Creator", UserId);

            return conn.Execute("Admin_BankFinancialReason_Ins", props, commandType: CommandType.StoredProcedure);
        }
    }
}
