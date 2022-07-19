using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using BackendSite.Extensions;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.BankCardManagement;
using BackendSite.Service.Model.enums;
using OfficeOpenXml.FormulaParsing.Excel.Operators;

namespace BackendSite.Service.DAL
{
    public class BankDetailService
    {
        private readonly IConfiguration configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly string _connectionString;

        public BankDetailService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this.configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }

        public IEnumerable<OfflineBankingSetting> GetOfflineBankingSettingList(GetOfflineBankingSettingParams input, out int totalPages, out int rowCount)
        {
            var para = new DynamicParameters();
            para.Add("_SiteId", input.SiteId);
            para.Add("_CurrencyId", input.CurrencyId);
            para.Add("_Purposes", JsonConvert.SerializeObject(input.Purposes));
            para.Add("_Statuses", JsonConvert.SerializeObject(input.Statuses));
            para.Add("_SysIds", JsonConvert.SerializeObject(input.SysIds));
            para.Add("_VipLevels", JsonConvert.SerializeObject(input.VipLevels));
            para.Add("_PointLevels", JsonConvert.SerializeObject(input.PointLevels));
            para.Add("_AccountNo", input.AccountNo ?? string.Empty);
            para.Add("_StartIndex", input.StartIndex);
            para.Add("_PageSize", input.PageSize);
            para.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<OfflineBankingSetting>("Admin_OfflineBankingSetting_Get", para);

            totalPages = para.Get<int?>("_TotalPages") ?? 0;
            rowCount = para.Get<int?>("_RowCount") ?? 0;

            return result;
        }
        
        public IEnumerable<BankDetail> GetLoaclBankList(string siteIds,
            string currencyIdList,
            string statusList, 
            int startIndex,int pageSize,
            out int totalPages, out int rowCount)
        {
            var props = new DynamicParameters();
            props.Add("_SiteId", siteIds);
            props.Add("_CurrencyId", currencyIdList);
            props.Add("_Status", statusList);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<BankDetail>("LoaclBankList_Get_V2", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;
        }

        public int SetBankCard(ParamsWithLog<SetBankCard> input)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", input.SiteId);
            props.Add("SysId", input.After.SysId);
            props.Add("BankAccountName", input.After.BankAccountName);
            props.Add("BankAccountNo", input.After.BankAccountNo);
            props.Add("MinAmount", input.After.MinAmount);
            props.Add("MaxAmount", input.After.MaxAmount);
            props.Add("DailyLimitAmount", input.After.DailyLimitAmount);
            props.Add("PointLevel", input.After.PointLevel);
            props.Add("Purpose", input.After.Purpose);
            props.Add("Status", input.After.Status);
            props.Add("Sort", input.After.Sort);
            #endregion

            #region propsLog
            var isCreate = input.Before.SysId == 0;
            propsLog.Add("_SiteId", input.SiteId);
            propsLog.Add("_CurrencyId", input.CurrencyId);
            propsLog.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int) (isCreate ? ActionType.Create : ActionType.Update));
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", isCreate ? null :input.Before.ToLog());
            propsLog.Add("_AfterDetailInfo", input.After.ToLog());
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r = _mainDbClient.Execute("BankCard_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
            return result;
        }

        public int SetBankCardwithIFSC(ParamsWithLog<SetBankCard> input)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", input.SiteId);
            props.Add("SysId", input.After.SysId);
            props.Add("BankAccountName", input.After.BankAccountName);
            props.Add("BankAccountNo", input.After.BankAccountNo);
            props.Add("IFSCCode", input.After.IFSCCode ?? string.Empty);
            props.Add("MinAmount", input.After.MinAmount);
            props.Add("MaxAmount", input.After.MaxAmount);
            props.Add("DailyLimitAmount", input.After.DailyLimitAmount);
            props.Add("PointLevel", input.After.PointLevel);
            props.Add("Purpose", input.After.Purpose);
            props.Add("Status", input.After.Status);
            props.Add("Sort", input.After.Sort);
            #endregion

            #region propsLog
            var isCreate = input.Before == null || input.Before.SysId == 0;
            propsLog.Add("_SiteId", input.SiteId);
            propsLog.Add("_CurrencyId", input.CurrencyId);
            propsLog.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int) (isCreate ? ActionType.Create : ActionType.Update));
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", isCreate ? null : input.Before.ToLog(), DbType.String);
            propsLog.Add("_AfterDetailInfo", input.After.ToLog());
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r = _mainDbClient.Execute("BankCardwithIFSC_Set", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
            return result;
        }
        public int DelBankCard(ParamsWithLog<DeleteBankCard> input)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            var props = new DynamicParameters();
            var propsLog = new DynamicParameters();

            #region props
            props.Add("SiteId", input.SiteId);
            props.Add("BankId", input.Before.BankId);
            #endregion

            #region propsLog
            propsLog.Add("_SiteId", input.SiteId);
            propsLog.Add("_CurrencyId", input.CurrencyId);
            propsLog.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BankCardManagement);
            propsLog.Add("_ActionType", (int) ActionType.Delete);
            propsLog.Add("_RefTransId", string.Empty);
            propsLog.Add("_BeforeDetailInfo", input.Before.ToLog());
            propsLog.Add("_AfterDetailInfo", null, DbType.String);
            propsLog.Add("_Operator", input.Operator);
            #endregion

            var r =  _mainDbClient.Execute("BankCard_Del", props, conn, trans);
            var result = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", propsLog, conn, trans);
            if (result > 0)
            {
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }
            return result;
        }
        public bool AddBankDetail(int siteId, BankDetail bankDetail)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("BankDetail_Ins", new { SiteId = siteId, SysId = bankDetail.SysId, BankAccountName = bankDetail.AccountName, BankAccountNo = bankDetail.AccountNo, MinAmount = bankDetail.MinAmount, MaxAmount = bankDetail.MaxAmount, PointLevel = bankDetail.PointLevel }, commandType: CommandType.StoredProcedure) > 0;
        }

        public bool EditBankDetail(int siteId, BankDetail bankDetail)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Execute("BankDetail_Set", new { SiteId = siteId, SysId = bankDetail.BankId, BankAccountName = bankDetail.AccountName, BankAccountNo = bankDetail.AccountNo, MinAmount = bankDetail.MinAmount, MaxAmount = bankDetail.MaxAmount, PointLevel = bankDetail.PointLevel }, commandType: CommandType.StoredProcedure) > 0;
        }

        public IEnumerable<BankInfoSys> GetSysBankList()
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<BankInfoSys>("BankInfoSysList_Get", new { SiteId = (int?)null }, commandType: CommandType.StoredProcedure);
        }
    }
}