using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    }
}
