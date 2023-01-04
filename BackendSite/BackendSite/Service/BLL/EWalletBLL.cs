using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class EWalletBLL
    {
        private readonly EWalletService _eWalletService;
        public EWalletBLL(IServiceProvider provider)
        {
            _eWalletService = provider.GetRequiredService<EWalletService>();
        }
        class BlockEWalletAccComparer : IEqualityComparer<BlockEWalletAcc>
        {
            public bool Equals(BlockEWalletAcc x, BlockEWalletAcc y)
            {
                if (Object.ReferenceEquals(x, y)) return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.AccountNum == y.AccountNum && x.SysId == y.SysId;
            }

            public int GetHashCode(BlockEWalletAcc obj)
            {
                if (object.ReferenceEquals(obj, null)) return 0;
                return obj.AccountNum == null ? 0 : obj.AccountNum.GetHashCode();
            }
        }

        public IEnumerable<BlockEWalletAcc> GetBlockEWalletAcc(DateTime startDate, DateTime endDate, int status, string accountNum, IEnumerable<int> currencyId, string userName, IEnumerable<int> eWalletId)
        {
            List<BlockEWalletAcc> custBlockEWalletAcc = _eWalletService.GetCustBlockEWalletAcc(startDate, endDate, status, accountNum, currencyId, userName, eWalletId).ToList();

            if (string.IsNullOrEmpty(userName))
            {
                List<BlockEWalletAcc> blockEWallet = _eWalletService.GetBlockEWallet(startDate, endDate, status, accountNum, currencyId, eWalletId).ToList();
                List<BlockEWalletAcc> noUseAcc = blockEWallet.Except(custBlockEWalletAcc, new BlockEWalletAccComparer()).ToList();
                custBlockEWalletAcc.AddRange(noUseAcc);
            }

            return custBlockEWalletAcc;
        }
        public IEnumerable<EWallet> GetBlockEWalletList(int sysId, string accountNum)
        {
            return _eWalletService.GetBlockEWalletList(sysId, accountNum);
        }
        public int BlockEWalletSet(ParamsWithLog<BlockEWalletSetRequest> input)
        {
            return _eWalletService.BlockEWalletSet(input);
        }
        public int ReleaseAccount(ParamsWithLog<ReleaseEWalletAccountRequest> input)
        {
            return _eWalletService.ReleaseAccount(input);
        }

        public IEnumerable<EWalletInfoSys> GetEWalletInfo()
        {
            return _eWalletService.GetEWalletInfo();
        }
    }
}
