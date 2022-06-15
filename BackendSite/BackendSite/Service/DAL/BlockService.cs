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
    public class BlockService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _ipTableClient;
        private readonly CustInfoService _custInfoService;
        public BlockService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _ipTableClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.IP);
            _custInfoService = provider.GetRequiredService<CustInfoService>();
        }
        public IEnumerable<BlockIP> GetBlockIPList(string ip, DateTime startDate,
            DateTime endDate, int startIndex,
            int pageSize, out int totalPages,out int rowCount)
        {
            var p = new DynamicParameters(new
            {
                IP = string.IsNullOrWhiteSpace(ip) ? null : ip,
                StartDate = startDate,
                EndDate = endDate,
                StartIndex = startIndex,
                PageSize = pageSize,
            });

            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result =  _reportDbClient.Query<BlockIP>("Admin_BlockIPList_Get", p);

            totalPages = p.Get<int?>("TotalPages") ?? 0;
            rowCount = p.Get<int?>("RowCount") ?? 0;

            return result;
        }

        public int SetBlockIP(string ip,string memo,string Operator)
        {
            var para = new DynamicParameters(new
            {
                IP = ip
            });
            var data =  _reportDbClient.Query<BlockIP>("Admin_BlockIP_Get", para).ToList();

            if(data.Count == 0)
            {
                var p = new DynamicParameters(new
                {
                    IP = ip,
                    Memo = memo,
                    Operator = Operator
                });
                var result = _mainDbClient.Execute("Admin_BlockIP_Set", p);
                return result;
            }
            else
            {
                return 0;
            }
        }

        public int DeleteBlockIP(string ip)
        {
            var p = new DynamicParameters(new
            {
                IP = ip
            });
            var result = _mainDbClient.Execute("Admin_BlockIP_Del", p);
            return result;
        }

        public IEnumerable<BlockBankAcc> GetBlockBankAcc(DateTime startDate,DateTime endDate, int status,string accountNum,string userName)
        {
            var jsonSelectStatus = string.Empty;
            if(status == 0)
            {
                var tempList = new List<int>() { 1, 2};
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else if(status == 1)
            {
                var tempList = new List<int>() { 1};
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
                AccountNum = string.IsNullOrWhiteSpace(accountNum) ? null: accountNum,
                UserName = string.IsNullOrWhiteSpace(userName) ? null : userName,
                SelectStatus = jsonSelectStatus,
            });

            return _reportDbClient.Query<BlockBankAcc>("Admin_BlockBankAcc_Get", p);
        }

        public IEnumerable<BlockList> GetBlockBankAccList(int sysId, string accountNum)
        {
            var p = new DynamicParameters(new
            {
                SysId = sysId,
                AccountNum = accountNum
            });

            return _reportDbClient.Query<BlockList>("Admin_BlockBankAccList_Get", p);
        }
        public IEnumerable<IPCountry> GetIPCountryList()
        {
            return _ipTableClient.Query<IPCountry>("Admin_IPCountry_Get");
        }

        public IEnumerable<IPUser> GetIPUserList(string ip)
        {
            var p = new DynamicParameters(new
            {
                LoginIp = ip
            });

            return _reportDbClient.Query<IPUser>("Admin_IPUserList_Get", p);
        }

        public int BlockAccountSet(int sysId, string accountNum, BlockAccountStatus status, string Operator, string memo = "")
        {
            var result = 0;
            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                _SysId = sysId,
                _AccountNum = accountNum,
                _Status = (int)status,
                _UpdateOperator = Operator,
                _Memo = memo
            });
            result = _mainDbClient.Execute("Admin_BlockAccount_Set", p, conn, trans);

            if(result > 0)
            {
                var blockBankAccBlockList = GetBlockBankAccList(sysId, accountNum).ToList();

                foreach(var x in blockBankAccBlockList)
                {
                    var parameter = new DynamicParameters(new
                    {
                        CustId = x.CustId,
                        SysId = sysId,
                        AccountNo = accountNum,
                        IFSCCode = x.IFSCCode,
                        Status = status == BlockAccountStatus.Lock ? (int)CustBankDetailStatus.Disable : (int)CustBankDetailStatus.Able
                    });

                    result = _mainDbClient.Execute("Member_CustBankDetail_Set", parameter, conn, trans);

                    if (result > 0)
                    {
                        var prop = new DynamicParameters(new
                        {
                            CustId = x.CustId,
                            SiteId = x.SiteId,
                            SysId = sysId,
                            AccountNum = accountNum,
                            IFSCCode = string.IsNullOrWhiteSpace(x.IFSCCode) ? "" : x.IFSCCode,
                            AccountName = accountNum,
                            Status = status == BlockAccountStatus.Lock ? CustBankInfoLogStatus.Lock : CustBankInfoLogStatus.Unlock
                        });

                        result = _mainDbClient.Execute("Admin_CustBankInfoLog_Set", prop, conn, trans);
                        if(result == 0)
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

        public int ReleaseAccount(int custId, int sysId, string accountNum, string iFSCCode)
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
            result = _mainDbClient.Execute("Admin_CustBankDetail_Del", p, conn, trans);
            
            if(result > 0)
            {
                var custInfo = _custInfoService.GetCustInfo(custId);

                if(custInfo != null)
                {
                    var prop = new DynamicParameters(new
                    {
                        CustId = custId,
                        SiteId = custInfo.SiteId,
                        SysId = sysId,
                        AccountNum = accountNum,
                        IFSCCode = string.IsNullOrWhiteSpace(iFSCCode) ? "" : iFSCCode,
                        AccountName = accountNum,
                        Status = CustBankInfoLogStatus.Release
                    });

                    result = _mainDbClient.Execute("Admin_CustBankInfoLog_Set", prop, conn, trans);

                    if(result <= 0)
                    {
                        trans.Rollback();
                    }
                }
                else
                {
                    trans.Rollback();
                }
                
            }
            else
            {
                trans.Rollback();
            }
            trans.Commit();
            return result;
        }
    }
}
