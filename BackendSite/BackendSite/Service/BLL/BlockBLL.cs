using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class BlockBLL
    {
        private readonly BlockService _blockService;
        private readonly CustInfoService _custInfoService;
        private readonly Tools _tools;
        public BlockBLL(IServiceProvider provider)
        {
            _blockService  = provider.GetRequiredService<BlockService>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
            _tools = provider.GetRequiredService<Tools>();
        }
        public IEnumerable<BlockIP> GetBlockIPList(string ip, DateTime startDate,
            DateTime endDate,int pageSize, int pageNumber,out int totalPages,out int rowCount)
        {
            var result =  _blockService.GetBlockIPList(ip, startDate, endDate, Math.Abs(pageSize) * (Math.Abs(pageNumber) - 1), Math.Abs(pageSize), out totalPages, out rowCount);
            if(result != null)
            {
                var ipCountryList = GetIPCountryList().ToList();
                result.ToList().ForEach(x =>
                {
                    var ip = _tools.getIpNum(x.IP);
                    var location = ipCountryList.SingleOrDefault(o => ip >= o.IPFrom && ip <= o.IPTo);
                    if(location != null)
                    {
                        x.Location = location.Country;
                    }
                });
            }
            return result;
        }
        public int SetBlockIP(string ip, string memo, string Operator)
        {
            return _blockService.SetBlockIP(ip, memo, Operator);
        }
        public int DeleteBlockIP(string ip)
        {
            return _blockService.DeleteBlockIP(ip);
        }
        public IEnumerable<BlockBankAcc> GetBlockBankAcc(DateTime startDate, DateTime endDate, int status, string accountNum, string userName)
        {
            return _blockService.GetBlockBankAcc(startDate, endDate, status, accountNum, userName);
        }
        public IEnumerable<BlockList> GetBlockBankAccList(int sysId, string accountNum)
        {
            return _blockService.GetBlockBankAccList(sysId, accountNum);
        }

        public int BlockAccountSet(int sysId, string accountNum, BlockAccountStatus status, string Operator, string memo)
        {
            return _blockService.BlockAccountSet(sysId, accountNum, status, Operator, memo);
        }
        public void SetCustBankInfoLog(int sysId, string accountNum,int status)
        {
            var blockBankAccBlockList  = _blockService.GetBlockBankAccList(sysId, accountNum).ToList();

            blockBankAccBlockList.ForEach(x =>
            {
                _custInfoService.SetCustBankInfoLog(x.CustId, x.SiteId, sysId, accountNum, x.IFSCCode, x.AccountName, status == 2 ? 3 : 4);
            });
        }
        private IEnumerable<IPCountry> GetIPCountryList()
        {
            return _blockService.GetIPCountryList();
        }
        public IEnumerable<IPUser> GetIPUserList(string ip)
        {
            return _blockService.GetIPUserList(ip);
        }

        public int ReleaseAccount(int custId, int sysId, string accountNum, string iFSCCode)
        {
            return _blockService.ReleaseAccount(custId, sysId, accountNum, iFSCCode);
        }
    }
}
