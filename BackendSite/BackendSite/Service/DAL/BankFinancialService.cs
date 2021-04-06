using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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
    public class BankFinancialService
    {

        #region Fields

        private readonly IConfiguration configuration;

        #endregion

        #region Constructors

        public BankFinancialService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #endregion

        #region Library Methods

        public IEnumerable<CurrencyModel> GetCurrencyList()
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                string SpName = "Common_Currency_Get";
                return conn.Query<CurrencyModel>(SpName, commandType: CommandType.StoredProcedure);
            }
        }
        
        public IEnumerable<BankFinancialReasonModel> GetReason(string funcKey)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {

                var parameters = new DynamicParameters(
                new
                {
                    _FuncKey = funcKey
                });

                string SpName = "Admin_BankFinancialReason_Get";
                return conn.Query<BankFinancialReasonModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        #endregion

        public IEnumerable<BankFinancialTransferAccountDetailModel> GetTransferAccountInfoList(bool isSetting, int siteId, int id)
        {

            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport")))
            {
                var parameters = new DynamicParameters(
                new
                {
                    _IsSetting = isSetting,
                    _SiteId = siteId,
                    _Id = id
                });

                string SpName = "Admin_BankFinancialTransferAccount_Get";
                return conn.Query<BankFinancialTransferAccountDetailModel>(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public int InsertTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var parameters = new DynamicParameters(
                new
                {
                    _SiteId = bankFinancialTransferAccount.SiteId,
                    _CurrencyId = bankFinancialTransferAccount.CurrencyId,
                    _SysId = bankFinancialTransferAccount.SysId,
                    _Purpose = bankFinancialTransferAccount.Purpose,
                    _Channel = bankFinancialTransferAccount.Channel,
                    _Agent = bankFinancialTransferAccount.Agent,
                    _PayId = bankFinancialTransferAccount.PayId,
                    _AccountNo = bankFinancialTransferAccount.AccountNo,
                    _AccountName = bankFinancialTransferAccount.AccountName,
                    _DisplayName = bankFinancialTransferAccount.DisplayName,
                    _InitialAmount = bankFinancialTransferAccount.InitialAmount,
                    _MaxBalanceAlertAmount = bankFinancialTransferAccount.MaxBalanceAlertAmount,
                    _Reason = bankFinancialTransferAccount.Reason,
                    _Remark = bankFinancialTransferAccount.Remark,
                    _Status = bankFinancialTransferAccount.Status,
                    _CreateTime = bankFinancialTransferAccount.CreateTime,
                    _Creator = bankFinancialTransferAccount.Creator,
                    _UpdateTime = bankFinancialTransferAccount.UpdateTime,
                    _Updator = bankFinancialTransferAccount.Updator
                });

                string SpName = "Admin_BankFinancialTransferAccount_Ins";

                return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public int UpdateTransferAccount(BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var parameters = new DynamicParameters(
                new
                {
                    _Id = bankFinancialTransferAccount.Id,
                    _SiteId = bankFinancialTransferAccount.SiteId,
                    _CurrencyId = bankFinancialTransferAccount.CurrencyId,
                    _SysId = bankFinancialTransferAccount.SysId,
                    _Purpose = bankFinancialTransferAccount.Purpose,
                    _Channel = bankFinancialTransferAccount.Channel,
                    _Agent = bankFinancialTransferAccount.Agent,
                    _PayId = bankFinancialTransferAccount.PayId,
                    _AccountNo = bankFinancialTransferAccount.AccountNo,
                    _AccountName = bankFinancialTransferAccount.AccountName,
                    _DisplayName = bankFinancialTransferAccount.DisplayName,
                    _InitialAmount = bankFinancialTransferAccount.InitialAmount,
                    _MaxBalanceAlertAmount = bankFinancialTransferAccount.MaxBalanceAlertAmount,
                    _Reason = bankFinancialTransferAccount.Reason,
                    _Remark = bankFinancialTransferAccount.Remark,
                    _Status = bankFinancialTransferAccount.Status,
                    _CreateTime = bankFinancialTransferAccount.CreateTime,
                    _Creator = bankFinancialTransferAccount.Creator,
                    _UpdateTime = bankFinancialTransferAccount.UpdateTime,
                    _Updator = bankFinancialTransferAccount.Updator
                });

                string SpName = "Admin_BankFinancialTransferAccount_Upd";

                return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public int InsertTransferAccountLog(ActionType action, BankFinancialTransferAccountModel bankFinancialTransferAccount)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                var parameters = new DynamicParameters(
                new
                {
                    _SiteId = bankFinancialTransferAccount.SiteId,
                    _CurrencyId = bankFinancialTransferAccount.CurrencyId,
                    _SysId = bankFinancialTransferAccount.SysId,
                    _Purpose = bankFinancialTransferAccount.Purpose,
                    _Channel = bankFinancialTransferAccount.Channel,
                    _Agent = bankFinancialTransferAccount.Agent,
                    _PayId = bankFinancialTransferAccount.PayId,
                    _AccountNo = bankFinancialTransferAccount.AccountNo,
                    _AccountName = bankFinancialTransferAccount.AccountName,
                    _DisplayName = bankFinancialTransferAccount.DisplayName,
                    _InitialAmount = bankFinancialTransferAccount.InitialAmount,
                    _MaxBalanceAlertAmount = bankFinancialTransferAccount.MaxBalanceAlertAmount,
                    _Reason = bankFinancialTransferAccount.Reason,
                    _Remark = bankFinancialTransferAccount.Remark,
                    _Status = bankFinancialTransferAccount.Status,
                    _OperateTime = bankFinancialTransferAccount.UpdateTime,
                    _Operator = bankFinancialTransferAccount.Updator,
                    _Action = action
                });

                string SpName = "Admin_BankFinancialTransferAccountLog_Ins";

                return conn.Execute(SpName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

    }
}
