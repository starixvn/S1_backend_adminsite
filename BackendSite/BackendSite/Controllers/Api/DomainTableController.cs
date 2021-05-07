using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    public class DomainTableController
    {
        private readonly DomainTableBLL domainTableBLL;
        private readonly AuthorizeBLL authorizeBLL;

        public DomainTableController(DomainTableBLL domainTableBLL, AuthorizeBLL authorizeBLL)
        {
            this.domainTableBLL = domainTableBLL;
            this.authorizeBLL = authorizeBLL;
        }

        public class DomainListReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public int Currency { get; set; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<DomainTable>> GetDomainList(DomainListReq domainListReq)
        {
            var apiResult = new ApiResult<IEnumerable<DomainTable>>();
            apiResult.Data = domainTableBLL.GetDomainList(domainListReq.SelectSite, domainListReq.Currency);
            apiResult.ErrorCode = 0;

            return apiResult;
        }
        public class SetDomainReq
        {
            public int SiteId { get; set; }
            public int Currency { get; set; }
            public string Domain { get; set; }
            public int Status { get; set; }
            public int Sort { get; set; }
        }

        [HttpPost]
        public ApiResult SetDomain(SetDomainReq setDomainReq)
        {
            ApiResult apiResult = new ApiResult();
            if (domainTableBLL.SetDomain(setDomainReq.SiteId, setDomainReq.Currency, setDomainReq.Domain, setDomainReq.Status, setDomainReq.Sort, authorizeBLL.GetAuthData().UserName) > 0)
            {
                apiResult.ErrorCode = 0;
            } else
            {
                apiResult.ErrorCode = 99;
            }
            return apiResult;
        }

        public class DelDomainReq
        {
            public int SiteId { get; set; }
            public int Currency { get; set; }
            public string Domain { get; set; }
        }

        [HttpPost]
        public ApiResult DelDomain(DelDomainReq delDomainReq)
        {
            ApiResult apiResult = new ApiResult();
            if (domainTableBLL.DelDomain(delDomainReq.SiteId, delDomainReq.Currency, delDomainReq.Domain) > 0)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = 99;
            }
            return apiResult;
        }

    }
}
