using System.Net;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class BlockBLL
    {
        private readonly BlockService _blockService;
        private readonly BlockIpToS3 _blockIpToS3;
        private readonly CustInfoService _custInfoService;
        private readonly Tools _tools;

        public BlockBLL(IServiceProvider provider)
        {
            _blockService = provider.GetRequiredService<BlockService>();
            _blockIpToS3 = provider.GetRequiredService<BlockIpToS3>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
            _tools = provider.GetRequiredService<Tools>();

        }
        class BlockBankAccComparer : IEqualityComparer<BlockBankAcc>
        {
            public bool Equals(BlockBankAcc x, BlockBankAcc y)
            {
                if (Object.ReferenceEquals(x, y)) return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.AccountNum == y.AccountNum && x.SysId == y.SysId;
            }

            public int GetHashCode(BlockBankAcc obj)
            {
                if (object.ReferenceEquals(obj, null)) return 0;
                return obj.AccountNum == null ? 0 : obj.AccountNum.GetHashCode();
            }
        }
        class BlockCryptoWalletComparer : IEqualityComparer<BlockCryptoWallet>
        {
            public bool Equals(BlockCryptoWallet x, BlockCryptoWallet y)
            {
                if (Object.ReferenceEquals(x, y)) return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.Address == y.Address;
            }

            public int GetHashCode(BlockCryptoWallet obj)
            {
                if (object.ReferenceEquals(obj, null)) return 0;
                return obj.Address == null ? 0 : obj.Address.GetHashCode();
            }
        }
        public async Task<BlockIPPageData> GetBlockIPList(string ip, DateTime startDate, DateTime endDate, int pageSize, int pageNumber)
        {
            return await _blockService.GetBlockIPList(ip, startDate, endDate, pageNumber, Math.Abs(pageSize));
        }
        public async Task<int> SetBlockIP(ParamsWithLog<SetBlockIPRequest> input)
        {
            if (!IPAddress.TryParse(input.After.IP, out _))
                return 0;

            var returnValue = _blockService.SetBlockIP(input);
            if (returnValue > 0)
            {
                returnValue = await _blockIpToS3.Upload();
            }
            return returnValue;
        }
        public async Task<int> DeleteBlockIP(ParamsWithLog<BlockIPRequest> input)
        {
            var returnValue = _blockService.DeleteBlockIP(input);
            if (returnValue > 0)
            {
                returnValue = await _blockIpToS3.Upload();
            }
            return returnValue;
        }
        public IEnumerable<BlockBankAcc> GetBlockBankAcc(DateTime startDate, DateTime endDate, int blockStatus, string accountNum, IEnumerable<int> currencyId, string userName)
        {
            List<BlockBankAcc> custBlockBankAcc = _blockService.GetCustBlockBankAcc(startDate, endDate, blockStatus, accountNum, currencyId, userName).ToList();

            if (string.IsNullOrEmpty(userName))
            {
                List<BlockBankAcc> blockBankAcc = _blockService.GetBlockAccount(startDate, endDate, blockStatus, accountNum, currencyId).ToList();
                List<BlockBankAcc> noUseAcc = blockBankAcc.Except(custBlockBankAcc, new BlockBankAccComparer()).ToList();
                custBlockBankAcc.AddRange(noUseAcc);
            }

            return custBlockBankAcc;
        }
        public IEnumerable<BlockList> GetBlockBankAccList(int sysId, string accountNum)
        {
            return _blockService.GetBlockBankAccList(sysId, accountNum);
        }

        public int BlockAccountSet(ParamsWithLog<BlockAccountSetRequest> input)
        {
            return _blockService.BlockAccountSet(input);
        }
        public IEnumerable<IPUser> GetIPUserList(string ip)
        {
            return _blockService.GetIPUserList(ip);
        }

        public int ReleaseAccount(ParamsWithLog<ReleaseAccountRequest> input)
        {
            return _blockService.ReleaseAccount(input);
        }

        public async Task<IEnumerable<DeviceUser>> GetDeviceUserList(int siteId, string deviceId)
        {
            if (siteId == 1)
                return await _blockService.GetIoavationAssociatedList(deviceId);
            return await _blockService.GetShieldAssociatedList(deviceId);
        }
        
        public async Task<IEnumerable<DeviceUser>> GetFPSDeviceUserList(int siteId,string deviceId)
        {
            return await _blockService.GetFPSAssociatedList(deviceId);
        }

        public IEnumerable<BlockCryptoWallet> GetBlockCryptoWallet(DateTime startDate, DateTime endDate, int status, string address, IEnumerable<int> currencyId, string userName, IEnumerable<int> cryptoType, IEnumerable<string> netWork, int siteId)
        {
            List<BlockCryptoWallet> custBlockCryptoWallet = _blockService.GetBlockCryptoWallet(startDate, endDate, status, address, currencyId, userName, cryptoType, netWork, siteId).ToList();

            if (string.IsNullOrEmpty(userName))
            {
                List<BlockCryptoWallet> blockCryptoAddress = _blockService.GetBlockCryptoAddress(startDate, endDate, status, address).ToList();
                List<BlockCryptoWallet> noUseAcc = blockCryptoAddress.Except(custBlockCryptoWallet, new BlockCryptoWalletComparer()).ToList();
                custBlockCryptoWallet.AddRange(noUseAcc);
            }
            return custBlockCryptoWallet;
        }

        public IEnumerable<BlockCryptoWalletList> GetBlockCryptoWalletList(string address)
        {
            return _blockService.GetBlockCryptoWalletList(address);
        }

        public int BlockCryptoWalletSet(ParamsWithLog<BlockCryptoWalletSetRequest> input)
        {
            return _blockService.BlockCryptoWalletSet(input);
        }

        public int ReleaseCryptoWallet(ParamsWithLog<ReleaseCryptoWalletRequest> input)
        {
            return _blockService.ReleaseCryptoWallet(input);
        }
    }
}
