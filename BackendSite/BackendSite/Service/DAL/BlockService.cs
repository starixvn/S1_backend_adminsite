using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class BlockService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public BlockService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }
        public IEnumerable<BlockIP> GetBlockIPList(string ip)
        {
            var p = new DynamicParameters(new
            {
                IP = string.IsNullOrWhiteSpace(ip) ? null : ip
            });
            return _reportDbClient.Query<BlockIP>("Admin_BlockIP_Get", p);
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

        public IEnumerable<BlockList> GetBlockBankAccBlockList(int sysId, string accountNum)
        {
            var p = new DynamicParameters(new
            {
                SysId = sysId,
                AccountNum = accountNum
            });

            return _reportDbClient.Query<BlockList>("Admin_BlockBankAccBlockList_Get", p);
        }

        public int BlockAccountSet(int sysId, string accountNum,int status,string Operator, string memo = "")
        {
            var p = new DynamicParameters(new
            {
                _SysId = sysId,
                _AccountNum = accountNum,
                _Status = status,
                _UpdateOperator = Operator,
                _Memo = memo
            });
            var result = _mainDbClient.Execute("Admin_BlockAccount_Set", p);
            return result;
        }
    }
}
