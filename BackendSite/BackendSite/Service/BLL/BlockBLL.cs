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
        public BlockBLL(IServiceProvider provider)
        {
            _blockService  = provider.GetRequiredService<BlockService>();
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
    }
}
