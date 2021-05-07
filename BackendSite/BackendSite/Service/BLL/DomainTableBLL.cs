using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class DomainTableBLL
    {
        private readonly DomainTableService domainTableService;

        public DomainTableBLL(DomainTableService domainTableService)
        {
            this.domainTableService = domainTableService;
        }
        public IEnumerable<DomainTable> GetDomainList(IEnumerable<int> selectSite, int currency)
        {
            return domainTableService.GetDomainList(selectSite, currency);
        }
        public int SetDomain(int siteId, int currency, string domain, int status, int sort, string lastUpdateUser)
        {
            return domainTableService.SetDomain( siteId, currency, domain, status, sort, lastUpdateUser);
        }
        public int DelDomain(int siteId, int currency, string domain)
        {
            return domainTableService.DelDomain( siteId, currency, domain);
        }
    }
}
