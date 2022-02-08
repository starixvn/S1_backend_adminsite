using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
        public IEnumerable<DomainTable> GetDomainListBySite(IEnumerable<int> siteId, IEnumerable<int> currency,
            int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return domainTableService.GetDomainListBySite(JsonConvert.SerializeObject(siteId),
                JsonConvert.SerializeObject(currency),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages, out rowCount);
        }
        public int SetDomain(int siteId, int currencyId, string domain, int status, int sort, string lastUpdateUser)
        {
            return domainTableService.SetDomain(siteId, currencyId, domain, status, sort, lastUpdateUser);
        }
        public int DelDomain(int dId)
        {
            return domainTableService.DelDomain(dId);
        }
        public int UpdateDomain(int dId, string domain, int status, int sort, string lastUpdateUser)
        {
            return domainTableService.UpdateDomain(dId, domain, status, sort, lastUpdateUser);
        }
    }
}
