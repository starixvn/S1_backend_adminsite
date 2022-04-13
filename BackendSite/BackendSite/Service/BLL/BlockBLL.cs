using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.DependencyInjection;
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
        public BlockBLL(IServiceProvider provider)
        {
            _blockService  = provider.GetRequiredService<BlockService>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
        }
        public IEnumerable<BlockIP> GetBlockIPList(string ip)
        {
            return _blockService.GetBlockIPList(ip);
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
        public IEnumerable<BlockList> GetBlockBankAccBlockList(int sysId, string accountNum)
        {
            return _blockService.GetBlockBankAccBlockList(sysId, accountNum);
        }

        public int BlockAccountSet(int sysId, string accountNum, int status, string Operator, string memo)
        {
            return _blockService.BlockAccountSet(sysId, accountNum, status, Operator, memo);
        }
        public void SetCustBankInfoLog(int sysId, string accountNum,int status)
        {
            var blockBankAccBlockList  = _blockService.GetBlockBankAccBlockList(sysId, accountNum).ToList();

            blockBankAccBlockList.ForEach(x =>
            {
                _custInfoService.SetCustBankInfoLog(x.CustId, x.SiteId, sysId, accountNum, x.IFSCCode, x.AccountName, status == 2 ? 3 : 4);
            });
        }
    }
}
