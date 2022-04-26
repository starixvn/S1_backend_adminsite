using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class WithdrawalLimitService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public WithdrawalLimitService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public int SetWithdrawalLimit(int custId, string userName, int siteId, decimal withdrawalLimit, string memo, string refTransId, out decimal afterWithdrawalLimit, MySqlConnection conn, MySqlTransaction trans)
        {
            var p = new DynamicParameters(new { CustId = custId, UserName = userName, SiteId = siteId, WithdrawalLimit = withdrawalLimit, Memo = memo, RefTransId = refTransId });
            p.Add("AfterWithdrawalLimit", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            var result = _mainDbClient.Execute("Common_WithdrawalLimit_Set", p, conn, trans);
            afterWithdrawalLimit = p.Get<decimal?>("AfterWithdrawalLimit") ?? 0;
            return result;
        }
        public IEnumerable<WithdrawalLimitInfoManage> GetWithdrawalLimitInfos(string siteId, string currencyId,int startIndex,int pageSize,out int totalPages,out int rowCount)
        {
            var props = new DynamicParameters();

            props.Add("_SiteIds", siteId);
            props.Add("_CurrencyIds", currencyId);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<WithdrawalLimitInfoManage>("Admin_WithdrawalLimitInfo_Get_V2", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;

        }

        public bool AddWithdrawalLimitInfo(ParamsWithLog<SetWithdrawalLimitInfo> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _VipLevel = input.After.VipLevel,
                _WithdrawalMin = input.After.WithdrawalMin,
                _WithdrawalMax = input.After.WithdrawalMax,
                _UpdateOperator = input.Operator,
                _Times = input.After.Times
            };
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.WithdrawalLimitation);
            logParam.Add("_ActionType", (int) ActionType.Create);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", null, DbType.String);
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            logParam.Add("_Operator", input.Operator);

            var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }
            
            var result = conn.Execute("Admin_WithdrawalLimitInfo_Ins_V2", param, commandType: CommandType.StoredProcedure) > 0;
            if (!result)
            {
                tran.Rollback();
                return false;
            }
            
            tran.Commit();

            return true;
        }

        public bool EditWithdrawalLimitInfo(ParamsWithLog<SetWithdrawalLimitInfo> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _VipLevel = input.After.VipLevel,
                _WithdrawalMin = input.After.WithdrawalMin,
                _WithdrawalMax = input.After.WithdrawalMax,
                _UpdateOperator = input.Operator,
                _Times = input.After.Times
            };
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.WithdrawalLimitation);
            logParam.Add("_ActionType", (int) ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            logParam.Add("_Operator", input.Operator);

            var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }
            
            var result = conn.Execute("Admin_WithdrawalLimitInfo_Set_V2", param, commandType: CommandType.StoredProcedure) > 0;
            if (!result)
            {
                tran.Rollback();
                return false;
            }
            
            tran.Commit();

            return true;
        }

        public WithdrawalLimitInfoManage GetWithdrawalLimitInfo(int siteId, int currencyId, int viplevel)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<WithdrawalLimitInfoManage>("Admin_WithdrawalLimitInfoDetail_Get", new { SiteId = siteId, CurrencyId = currencyId, VipLevel = viplevel }, commandType: CommandType.StoredProcedure);
        }

        public bool DeleteWithdrawalLimitInfo(ParamsWithLog<DeleteWithdrawalLimitInfo> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                input.SiteId,
                input.CurrencyId,
                input.Before.VipLevel
            };
            
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.WithdrawalLimitation);
            logParam.Add("_ActionType", (int) ActionType.Delete);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            logParam.Add("_AfterDetailInfo", null, DbType.String);
            logParam.Add("_Operator", input.Operator);

            var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }
            
            var result = conn.Execute("Admin_WithdrawalLimitInfo_Del", param, commandType: CommandType.StoredProcedure) > 0;
            if (!result)
            {
                tran.Rollback();
                return false;
            }
            
            tran.Commit();

            return true;
        }

        public IEnumerable<USDTWithdrawalLimitInfoManage> GetUSDTWithdrawalLimitInfos(string siteId, string currencyId,
            string Network,int startIndex,int pageSize,out int totalPages,out int rowCount)
        {
            var props = new DynamicParameters();

            props.Add("_SiteIds", siteId);
            props.Add("_CurrencyIds", currencyId);
            props.Add("_Network", Network);
            props.Add("_StartIndex", startIndex);
            props.Add("_PageSize", pageSize);
            props.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            props.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<USDTWithdrawalLimitInfoManage>("Admin_USDTWithdrawalLimitInfo_Get_V3", props);

            totalPages = props.Get<int?>("_TotalPages") ?? 0;
            rowCount = props.Get<int?>("_RowCount") ?? 0;

            return result;

        }

        public bool AddUSDTWithdrawalLimitInfo(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage, string UpdateOperator)
        {
            var result = false;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                USDTwithdrawalLimitInfoManage.SiteId,
                USDTwithdrawalLimitInfoManage.CurrencyId,
                USDTwithdrawalLimitInfoManage.VipLevel,
                USDTwithdrawalLimitInfoManage.WithdrawalMin,
                USDTwithdrawalLimitInfoManage.WithdrawalMax,
                UpdateOperator,
                Network = Enum.GetName(typeof(NetworkTypeEnum), USDTwithdrawalLimitInfoManage.NetworkType)
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "USDTWithdrawalLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator = UpdateOperator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Ins", param, commandType: CommandType.StoredProcedure) > 0;
            tran.Commit();

            return result;
        }

        public USDTWithdrawalLimitInfoManage GetUSDTWithdrawalLimitInfo(int siteId, int currencyId, int viplevel, int Network)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.QueryFirstOrDefault<USDTWithdrawalLimitInfoManage>("Admin_USDTWithdrawalLimitInfoDetail_Get", new { SiteId = siteId, CurrencyId = currencyId, VipLevel = viplevel, Network  = Network }, commandType: CommandType.StoredProcedure);
        }

        public int DeleteUSDTWithdrawalLimitInfo(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage, string Operator)
        {
            var result = 0;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                USDTwithdrawalLimitInfoManage.SiteId,
                USDTwithdrawalLimitInfoManage.CurrencyId,
                USDTwithdrawalLimitInfoManage.VipLevel,
                Network = Enum.GetName(typeof(NetworkTypeEnum), USDTwithdrawalLimitInfoManage.NetworkType)
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "USDTWithdrawalLimitManagement", ActionType = 2, DetailInfo = JsonConvert.SerializeObject(param), Operator = Operator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Del", new { SiteId = USDTwithdrawalLimitInfoManage.SiteId, CurrencyId = USDTwithdrawalLimitInfoManage.CurrencyId, VipLevel = USDTwithdrawalLimitInfoManage.VipLevel , Network = Enum.GetName(typeof(NetworkTypeEnum), USDTwithdrawalLimitInfoManage.NetworkType) }, commandType: CommandType.StoredProcedure);
            tran.Commit();

            return result;
        }

        public bool EditUSDTWithdrawalLimitInfo(USDTWithdrawalLimitInfoManage USDTwithdrawalLimitInfoManage, string UpdateOperator)
        {
            var result = false;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                USDTwithdrawalLimitInfoManage.SiteId,
                USDTwithdrawalLimitInfoManage.CurrencyId,
                USDTwithdrawalLimitInfoManage.VipLevel,
                USDTwithdrawalLimitInfoManage.WithdrawalMin,
                USDTwithdrawalLimitInfoManage.WithdrawalMax,
                UpdateOperator,
                Network = Enum.GetName(typeof(NetworkTypeEnum), USDTwithdrawalLimitInfoManage.NetworkType)
            };

            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "USDTWithdrawalLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator = UpdateOperator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Set", param, commandType: CommandType.StoredProcedure) > 0;
            tran.Commit();

            return result;
        }

        public IEnumerable<CryptoWithdrawalLimitInfo> GetCryptoWithdrawalLimitInfos(string siteIds,
            string currencyIds,
            int cryptoCurrencyId,
            string network,
            int startIndex, 
            int pageSize, 
            out int totalPages, 
            out int rowCount)
        {
            var param = new DynamicParameters();

            param.Add("_SiteIds", siteIds);
            param.Add("_CurrencyIds", currencyIds);
            param.Add("_CryptoCurrencyId", cryptoCurrencyId);
            param.Add("_Network", network);
            param.Add("_StartIndex", startIndex);
            param.Add("_PageSize", pageSize);
            param.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            param.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result =
                _reportDbClient.Query<CryptoWithdrawalLimitInfo>("Admin_USDTWithdrawalLimitInfo_Get_V4", param);

            totalPages = param.Get<int?>("_TotalPages") ?? 0;
            rowCount = param.Get<int?>("_RowCount") ?? 0;

            return result;

        }

        public bool AddCryptoWithdrawalLimitInfo(ParamsWithLog<CreateCryptoWithdrawalLimitInfo> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            var insParam = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _WithdrawalMin = input.After.WithdrawalMin,
                _WithdrawalMax = input.After.WithdrawalMax,
                _UpdateOperator = input.Operator,
                _Network = input.After.NetworkType.ToString(),
                _CryptoCurrencyId = (int) input.After.CryptoCurrency,
                _Times = input.After.Times
            };

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.CryptoWithdrawalLimitation);
            logParam.Add("_ActionType", (int) ActionType.Create);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", null, DbType.String);
            logParam.Add("_AfterDetailInfo", JsonConvert.SerializeObject(input.After));
            logParam.Add("_Operator", input.Operator);

            var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }

            var result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Ins_V4", insParam, tran, commandType: CommandType.StoredProcedure) > 0;

            if (!result)
            {
                tran.Rollback();
                return false;
            }

            tran.Commit();

            return true;
        }

        public CryptoWithdrawalLimitInfoManage GetCryptoWithdrawalLimitInfo(CryptoWithdrawalLimitInfoManage input, int siteId, int currencyId)
        {
            var param = new DynamicParameters();

            param.Add("_SiteId", siteId);
            param.Add("_CurrencyId", currencyId);
            param.Add("_CryptoCurrencyId", (int)input.CryptoCurrency);
            param.Add("_Network", (int)input.NetworkType);
            param.Add("_VipLevel", input.VipLevel);

            return _reportDbClient.QueryFirst<CryptoWithdrawalLimitInfoManage>("Admin_USDTWithdrawalLimitInfoDetail_Get_V2",
                param);
        }

        public bool DeleteCryptoWithdrawalLimitInfo(ParamsWithLog<DeleteCryptoWithdrawalLimitInfo> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            var insParam = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _VipLevel = input.Before.VipLevel,
                _Network = input.Before.NetworkType.ToString(),
                _CryptoCurrencyId = (int)input.Before.CryptoCurrency
            };

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.CryptoWithdrawalLimitation);
            logParam.Add("_ActionType", (int) ActionType.Delete);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", JsonConvert.SerializeObject(input.Before));
            logParam.Add("_AfterDetailInfo", null, DbType.String);
            logParam.Add("_Operator", input.Operator);

            var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }

            var result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Del_V2", insParam, tran, commandType: CommandType.StoredProcedure) > 0;
            
            if (!result)
            {
                tran.Rollback();
                return false;
            }

            tran.Commit();

            return true;
        }

        public bool EditCryptoWithdrawalLimitInfo(ParamsWithLog<UpdateCryptoWithdrawalLimitInfo> input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            var insParam = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _VipLevel = input.After.VipLevel,
                _WithdrawalMin = input.After.WithdrawalMin,
                _WithdrawalMax = input.After.WithdrawalMax,
                _UpdateOperator = input.Operator,
                _Network = input.After.NetworkType.ToString(),
                _CryptoCurrencyId = (int) input.After.CryptoCurrency,
                _Times = input.After.Times
            };

            var logParam = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _Menu = (int) AdminAdjustmentLogMenuEnum.CryptoWithdrawalLimitation,
                _ActionType = (int) ActionType.Update,
                _RefTransId = string.Empty,
                _BeforeDetailInfo = JsonConvert.SerializeObject(input.Before),
                _AfterDetailInfo = JsonConvert.SerializeObject(input.After),
                _Operator = input.Operator
            };

            var logResult = conn.Execute("Admin_AdminAdjustmentInfoLog_Ins", logParam, tran, commandType: CommandType.StoredProcedure) > 0;

            if (!logResult)
            {
                tran.Rollback();
                return false;
            }

            var result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Set_V3", insParam, tran, commandType: CommandType.StoredProcedure) > 0;

            if (!result)
            {
                tran.Rollback();
                return false;
            }

            tran.Commit();

            return true;
        }

        public IEnumerable<CryptoInfo> GetCryptoInfoList()
            => _reportDbClient.Query<CryptoInfo>("Admin_CryptoInfo_Get");
    }
}
