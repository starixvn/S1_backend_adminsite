using BackendSite.Service.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class CustInfoBLL
    {
        private readonly CustInfoService custInfoService;
        private readonly ServerInfoBLL serverInfoBLL;

        public CustInfoBLL(CustInfoService custInfoService, ServerInfoBLL serverInfoBLL)
        {
            this.custInfoService = custInfoService;
            this.serverInfoBLL = serverInfoBLL;
        }

        public IEnumerable<dynamic> GetCustInfoList(string userName, string transId, int pageSize, ref int pageNumber, out int totalPages)
        {
            var serverInfo = serverInfoBLL.GetServerInfo();
            return custInfoService.GetCustInfoList(userName, transId, serverInfo.SiteId, pageSize, ref pageNumber, out totalPages);
        }

        public dynamic GetCustInfo(int custId)
        {
            var serverInfo = serverInfoBLL.GetServerInfo();
            return custInfoService.GetCustInfo(custId, serverInfo.SiteId);
        }
    }
}
