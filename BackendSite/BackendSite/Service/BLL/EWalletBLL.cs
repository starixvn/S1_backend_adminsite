using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class EWalletBLL
    {
        private readonly EWalletService _eWalletService;
        public EWalletBLL(IServiceProvider provider)
        {
            _eWalletService = provider.GetRequiredService<EWalletService>();
        }
        public IEnumerable<BlockEWalletAcc> GetBlockEWalletAcc(DateTime startDate, DateTime endDate, int status, string accountNum, IEnumerable<int> currencyId, string userName, IEnumerable<int> eWalletId)
        {
            return _eWalletService.GetBlockEWalletAcc(startDate, endDate, status, accountNum, currencyId, userName, eWalletId);
        }
        public IEnumerable<EWalletList> GetBlockEWalletList(int sysId, string accountNum)
        {
            return _eWalletService.GetBlockEWalletList(sysId, accountNum);
        }
        public int BlockEWalletSet(int sysId, string accountNum, BlockAccountStatus status, string Operator, string memo)
        {
            return _eWalletService.BlockEWalletSet(sysId, accountNum, status, Operator, memo);
        }
        public int ReleaseAccount(int custId, int sysId, string accountNum)
        {
            return _eWalletService.ReleaseAccount(custId, sysId, accountNum);
        }
    }
}
