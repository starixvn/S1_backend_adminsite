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
        public IEnumerable<WithdrawalLimitInfoManage> GetWithdrawalLimitInfos(int siteId, int currencyId)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.Query<WithdrawalLimitInfoManage>("Admin_WithdrawalLimitInfo_Get", new { SiteId = siteId, CurrencyId = currencyId }, commandType: CommandType.StoredProcedure);
        }

        public bool AddWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string UpdateOperator)
        {
            var result = false;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                withdrawalLimitInfoManage.SiteId,
                withdrawalLimitInfoManage.CurrencyId,
                withdrawalLimitInfoManage.VipLevel,
                withdrawalLimitInfoManage.WithdrawalMin,
                withdrawalLimitInfoManage.WithdrawalMax,
                UpdateOperator
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "WithdrawalLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator = UpdateOperator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_WithdrawalLimitInfo_Ins", param, commandType: CommandType.StoredProcedure) > 0;
            tran.Commit();

            return result;
        }

        public bool EditWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage,string UpdateOperator)
        {
            var result = false;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                withdrawalLimitInfoManage.SiteId,
                withdrawalLimitInfoManage.CurrencyId,
                withdrawalLimitInfoManage.VipLevel,
                withdrawalLimitInfoManage.WithdrawalMin,
                withdrawalLimitInfoManage.WithdrawalMax,
                UpdateOperator
            };

            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "WithdrawalLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator= UpdateOperator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_WithdrawalLimitInfo_Set", param, commandType: CommandType.StoredProcedure) > 0;
            tran.Commit();

            return result;
        }

        public WithdrawalLimitInfoManage GetWithdrawalLimitInfo(int siteId, int currencyId, int viplevel)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            return conn.QueryFirstOrDefault<WithdrawalLimitInfoManage>("Admin_WithdrawalLimitInfoDetail_Get", new { SiteId = siteId, CurrencyId = currencyId, VipLevel = viplevel }, commandType: CommandType.StoredProcedure);
        }

        public int DeleteWithdrawalLimitInfo(WithdrawalLimitInfoManage withdrawalLimitInfoManage, string Operator)
        {
            var result = 0;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var tran = conn.BeginTransaction();
            var param = new
            {
                withdrawalLimitInfoManage.SiteId,
                withdrawalLimitInfoManage.CurrencyId,
                withdrawalLimitInfoManage.VipLevel
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "WithdrawalLimitManagement", ActionType = 2, DetailInfo = JsonConvert.SerializeObject(param), Operator = Operator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_WithdrawalLimitInfo_Del", new { SiteId = withdrawalLimitInfoManage.SiteId, CurrencyId = withdrawalLimitInfoManage.CurrencyId, VipLevel = withdrawalLimitInfoManage.VipLevel }, commandType: CommandType.StoredProcedure);
            tran.Commit();

            return result;
        }

        public IEnumerable<USDTWithdrawalLimitInfoManage> GetUSDTWithdrawalLimitInfos(int siteId, int currencyId, string Network)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<USDTWithdrawalLimitInfoManage>("Admin_USDTWithdrawalLimitInfo_Get", new { SiteId = siteId, CurrencyId = currencyId, Network= Network }, commandType: CommandType.StoredProcedure);
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
                Network = Enum.GetName(typeof(NetworkType), USDTwithdrawalLimitInfoManage.NetworkType)
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "USDTWithdrawalLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator = UpdateOperator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Ins", param, commandType: CommandType.StoredProcedure) > 0;
            tran.Commit();

            return result;
        }

        public USDTWithdrawalLimitInfoManage GetUSDTWithdrawalLimitInfo(int siteId, int currencyId, int viplevel,string Network)
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
                Network = Enum.GetName(typeof(NetworkType), USDTwithdrawalLimitInfoManage.NetworkType)
            };
            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "USDTWithdrawalLimitManagement", ActionType = 2, DetailInfo = JsonConvert.SerializeObject(param), Operator = Operator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Del", new { SiteId = USDTwithdrawalLimitInfoManage.SiteId, CurrencyId = USDTwithdrawalLimitInfoManage.CurrencyId, VipLevel = USDTwithdrawalLimitInfoManage.VipLevel , Network = Enum.GetName(typeof(NetworkType), USDTwithdrawalLimitInfoManage.NetworkType) }, commandType: CommandType.StoredProcedure);
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
                Network = Enum.GetName(typeof(NetworkType), USDTwithdrawalLimitInfoManage.NetworkType)
            };

            conn.Execute("Admin_AdjustmentInfoLog_Set", new { Menu = "USDTWithdrawalLimitManagement", ActionType = 1, DetailInfo = JsonConvert.SerializeObject(param), Operator = UpdateOperator }, tran, commandType: CommandType.StoredProcedure);

            result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Set", param, commandType: CommandType.StoredProcedure) > 0;
            tran.Commit();

            return result;
        }

        public IEnumerable<CryptoWithdrawalLimitInfoManage> GetCryptoWithdrawalLimitInfos(string siteIds,
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
                _reportDbClient.Query<CryptoWithdrawalLimitInfoManage>("Admin_USDTWithdrawalLimitInfo_Get_V4", param);

            totalPages = param.Get<int?>("_TotalPages") ?? 0;
            rowCount = param.Get<int?>("_RowCount") ?? 0;

            return result;

        }

        public bool AddCryptoWithdrawalLimitInfo(CryptoWithdrawalLimitInfoManage input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            var insParam = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _VipLevel = input.VipLevel,
                _WithdrawalMin = input.WithdrawalMin,
                _WithdrawalMax = input.WithdrawalMax,
                _UpdateOperator = input.UpdateOperator,
                _CryptoCurrencyId = input.CryptoCurrencyId,
                _Network = Enum.GetName(typeof(NetworkTypeEnum), input.NetworkType)
            };

            var logParam = new
            {
                Menu = "USDTWithdrawalLimitManagement",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(insParam),
                Operator = input.UpdateOperator
            };

            var logResult = conn.Execute("Admin_AdjustmentInfoLog_Set", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            
            if (!logResult)
            {
                tran.Rollback();
                return false;
            }

            var result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Ins_V2", insParam, tran, commandType: CommandType.StoredProcedure) > 0;

            if (!result)
            {
                tran.Rollback();
                return false;
            }

            tran.Commit();

            return true;
        }

        public CryptoWithdrawalLimitInfoManage GetCryptoWithdrawalLimitInfo(CryptoWithdrawalLimitInfoManage input)
        {
            var param = new DynamicParameters();

            param.Add("_SiteId", input.SiteId);
            param.Add("_CurrencyId", input.CurrencyId);
            param.Add("_CryptoCurrencyId", input.CryptoCurrencyId);
            param.Add("_Network", input.Network);
            param.Add("_VipLevel", input.VipLevel);

            return _reportDbClient.QueryFirst<CryptoWithdrawalLimitInfoManage>("Admin_USDTWithdrawalLimitInfoDetail_Get_V2",
                param);
        }

        public bool DeleteCryptoWithdrawalLimitInfo(CryptoWithdrawalLimitInfoManage input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            var insParam = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _VipLevel = input.VipLevel,
                _Network = Enum.GetName(typeof(NetworkTypeEnum), input.NetworkType),
                _CryptoCurrencyId = input.CryptoCurrencyId
            };

            var logParam = new
            {
                Menu = "USDTWithdrawalLimitManagement",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(insParam),
                Operator = input.UpdateOperator
            };

            var logResult = conn.Execute("Admin_AdjustmentInfoLog_Set", logParam, tran, commandType: CommandType.StoredProcedure) > 0;
            
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

        public bool EditCryptoWithdrawalLimitInfo(CryptoWithdrawalLimitInfoManage input)
        {
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();

            using var tran = conn.BeginTransaction();
            var insParam = new
            {
                _SiteId = input.SiteId,
                _CurrencyId = input.CurrencyId,
                _VipLevel = input.VipLevel,
                _WithdrawalMin = input.WithdrawalMin,
                _WithdrawalMax = input.WithdrawalMax,
                _UpdateOperator = input.UpdateOperator,
                _Network = Enum.GetName(typeof(NetworkTypeEnum), input.NetworkType),
                _CryptoCurrencyId = input.CryptoCurrencyId
            };

            var logParam = new
            {
                Menu = "USDTWithdrawalLimitManagement",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(insParam),
                Operator = input.UpdateOperator
            };

            var logResult = conn.Execute("Admin_AdjustmentInfoLog_Set", logParam, tran, commandType: CommandType.StoredProcedure) > 0;

            if (!logResult)
            {
                tran.Rollback();
                return false;
            }

            var result = conn.Execute("Admin_USDTWithdrawalLimitInfo_Set_V2", insParam, tran, commandType: CommandType.StoredProcedure) > 0;

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
