using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class EWalletService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _ipTableClient;
        private readonly CustInfoService _custInfoService;
        public EWalletService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _custInfoService = provider.GetRequiredService<CustInfoService>();
        }

        public IEnumerable<BlockEWalletAcc> GetBlockEWalletAcc(DateTime startDate, DateTime endDate, int status, string accountNum, IEnumerable<int> currencyId, string userName, IEnumerable<int> eWalletId)
        {
            var jsonSelectStatus = string.Empty;
            if (status == 0)
            {
                var tempList = new List<int>() { 1, 2 };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else if (status == 1)
            {
                var tempList = new List<int>() { 1 };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else
            {
                var tempList = new List<int>() { 2 };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }

            var p = new DynamicParameters(new
            {
                StartDate = startDate,
                EndDate = endDate,
                AccountNum = string.IsNullOrWhiteSpace(accountNum) ? null : accountNum,
                UserName = string.IsNullOrWhiteSpace(userName) ? null : userName,
                SelectStatus = jsonSelectStatus,
                SelectCurrency = JsonConvert.SerializeObject(currencyId),
                SelectSysId = JsonConvert.SerializeObject(eWalletId)
            });

            return _reportDbClient.Query<BlockEWalletAcc>("Admin_BlockEWalletAcc_Get", p);
        }

        public IEnumerable<EWallet> GetBlockEWalletList(int sysId, string accountNum)
        {
            var p = new DynamicParameters(new
            {
                SysId = sysId,
                AccountNum = accountNum
            });

            return _reportDbClient.Query<EWallet>("Admin_BlockEWalletList_Get", p);
        }

        public int BlockEWalletSet(int sysId, string accountNum, BlockAccountStatus status, string Operator, string memo = "")
        {
            var result = 0;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                SysId = sysId,
                AccountNum = accountNum,
                Status = (int)status,
                UpdateOperator = Operator,
                Memo = memo
            });
            result = _mainDbClient.Execute("Admin_BlockEWallet_Set", p, conn, trans);

            if (result > 0)
            {
                var blockEWalletList = GetBlockEWalletList(sysId, accountNum).ToList();

                foreach (var x in blockEWalletList)
                {
                    var parameter = new DynamicParameters(new
                    {
                        CustId = x.CustId,
                        SysId = sysId,
                        AccountNo = accountNum,
                        Status = status == BlockAccountStatus.Lock ? (int)CustBankDetailStatus.Disable : (int)CustBankDetailStatus.BODelete
                    });

                    result = _mainDbClient.Execute("Member_CustEWalletDetail_Set", parameter, conn, trans);

                    if (result > 0)
                    {
                        var prop = new DynamicParameters(new
                        {
                            CustId = x.CustId,
                            SiteId = x.SiteId,
                            SysId = sysId,
                            AccountNum = accountNum,
                            AccountName = x.AccountName,
                            Status = status == BlockAccountStatus.Lock ? (int)EWalletinfoLogStatusEnum.Lock : (int)EWalletinfoLogStatusEnum.Unlock
                        });

                        result = _mainDbClient.Execute("Admin_EWalletinfoLog_Set", prop, conn, trans);
                        if (result == 0)
                        {
                            trans.Rollback();
                            return result;
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return result;
                    }
                }
                trans.Commit();
            }
            else
            {
                trans.Rollback();
            }

            return result;
        }

        public int ReleaseAccount(int custId, int sysId, string accountNum)
        {
            var result = 0;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                CustId = custId,
                SysId = sysId,
                AccountNo = accountNum
            });
            var releaseEWalletList = GetBlockEWalletList(sysId, accountNum).ToList();
            result = _mainDbClient.Execute("Admin_CustEWalletDetail_Del", p, conn, trans);

            if (result > 0)
            {
                foreach (var x in releaseEWalletList)
                {
                    var prop = new DynamicParameters(new
                    {
                        CustId = x.CustId,
                        SiteId = x.SiteId,
                        SysId = sysId,
                        AccountNum = accountNum,
                        AccountName = x.AccountName,
                        Status = (int)EWalletinfoLogStatusEnum.Release
                    });
                    result = _mainDbClient.Execute("Admin_EWalletinfoLog_Set", prop, conn, trans);
                    if (result == 0)
                    {
                        trans.Rollback();
                        return result;
                    }
                }
            }
            else
            {
                trans.Rollback();
                return result;
            }
            trans.Commit();
            return result;
        }

        public IEnumerable<EWalletInfoSys> GetEWalletInfo()
        {
            return _reportDbClient.Query<EWalletInfoSys>("Admin_EWalletInfoSys_Get");
        }
    }
}
