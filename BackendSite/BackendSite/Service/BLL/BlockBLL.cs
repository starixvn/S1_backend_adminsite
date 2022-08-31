using System.Net;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;

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
            _blockService  = provider.GetRequiredService<BlockService>();
            _blockIpToS3 = provider.GetRequiredService<BlockIpToS3>();
            _custInfoService = provider.GetRequiredService<CustInfoService>();
            _tools = provider.GetRequiredService<Tools>();
            
        }
        public IEnumerable<BlockIP> GetBlockIPList(string ip, DateTime startDate,
            DateTime endDate,int pageSize, int pageNumber,out int totalPages,out int rowCount)
        {
            var result =  _blockService.GetBlockIPList(ip, startDate, endDate, Math.Abs(pageSize) * (Math.Abs(pageNumber) - 1), Math.Abs(pageSize), out totalPages, out rowCount);
            return result;
        }
        public async Task<int> SetBlockIP(ParamsWithLog<SetBlockIPRequest> input)
        {
            if (!IPAddress.TryParse(input.After.IP, out _))
                return 0;

            var returnValue = await _blockIpToS3.Upload(input.After.IP);
            if (returnValue > 0)
            {
                returnValue = _blockService.SetBlockIP(input);
            }
            return returnValue;
        }
        public async Task<int> DeleteBlockIP(ParamsWithLog<BlockIPRequest> input)
        {
            var returnValue = await _blockIpToS3.DeleteIp(input.Before.IP);
            if (returnValue > 0)
            {
                returnValue = _blockService.DeleteBlockIP(input);
            }
            return returnValue;
        }
        public IEnumerable<BlockBankAcc> GetBlockBankAcc(DateTime startDate, DateTime endDate, int blockStatus, string accountNum, IEnumerable<int> currencyId, string userName)
        {
            return _blockService.GetBlockBankAcc(startDate, endDate, blockStatus, accountNum, currencyId, userName);
        }
        public IEnumerable<BlockList> GetBlockBankAccList(int sysId, string accountNum)
        {
            return _blockService.GetBlockBankAccList(sysId, accountNum);
        }

        public int BlockAccountSet(ParamsWithLog<BlockAccountSetRequest> input)
        {
            return _blockService.BlockAccountSet(input);
        }
        private IEnumerable<IPCountry> GetIPCountryList()
        {
            return _blockService.GetIPCountryList();
        }
        public IEnumerable<IPUser> GetIPUserList(string ip)
        {
            return _blockService.GetIPUserList(ip);
        }

        public int ReleaseAccount(ParamsWithLog<ReleaseAccountRequest> input)
        {
            return _blockService.ReleaseAccount(input);
        }
    }
}
