using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DomainTableController
    {
        private readonly DomainTableBLL domainTableBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly SelLangBLL selLangBLL;

        public DomainTableController(DomainTableBLL domainTableBLL, AuthorizeBLL authorizeBLL, SelLangBLL selLangBLL)
        {
            this.domainTableBLL = domainTableBLL;
            this.authorizeBLL = authorizeBLL;
            this.selLangBLL = selLangBLL;
        }

        public class DomainListReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public int CurrencyId { get; set; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<DomainTable>> GetDomainList(DomainListReq domainListReq)
        {
            var apiResult = new ApiResult<IEnumerable<DomainTable>>
            {
                Data = domainTableBLL.GetDomainList(domainListReq.SelectSite, domainListReq.CurrencyId),
                ErrorCode = 0
            };

            return apiResult;
        }

        public class DomainListbySiteReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<DomainTable>> GetDomainListbySite(DomainListbySiteReq domainListbySiteReq)
        {
            var apiResult = new ApiResult<IEnumerable<DomainTable>>
            {
                Data = domainTableBLL.GetDomainListbySite(domainListbySiteReq.SiteId, domainListbySiteReq.CurrencyId),
                ErrorCode = 0
            };

            return apiResult;
        }
        public class SetDomainReq
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public string Domain { get; set; }
            public int Status { get; set; }
            public int Sort { get; set; }
        }

        [HttpPost]
        public ApiResult SetDomain(SetDomainReq setDomainReq)
        {
            var apiResult = new ApiResult();
            try
            {
                if (domainTableBLL.SetDomain(setDomainReq.SiteId, setDomainReq.CurrencyId, setDomainReq.Domain, setDomainReq.Status, setDomainReq.Sort, authorizeBLL.GetAuthData().UserName) > 0)
                {
                    apiResult.ErrorCode = 0;
                    apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
                }
                else
                {
                    apiResult.ErrorCode = 9;
                    apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                }
            }
            catch (MySqlException ex)
            {

                if (ex.Number == 1062) //匯入資料重複
                {
                    apiResult.ErrorCode = ex.Number;
                    apiResult.Message = "Duplicate data";
                }
                else
                {
                    apiResult.ErrorCode = ex.Number;
                    apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode =91;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        public class DelDomainReq
        {
            public int DId { get; set; }
        }

        [HttpPost]
        public ApiResult DelDomain(DelDomainReq delDomainReq)
        {
            var apiResult = new ApiResult();
            if (domainTableBLL.DelDomain(delDomainReq.DId) > 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        public class UpdateDomainReq
        {
            public int DId { get; set; }
            public string Domain { get; set; }
            public int Status { get; set; }
            public int Sort { get; set; }
        }

        [HttpPost]
        public ApiResult UpdateDomain(UpdateDomainReq updateDomainReq)
        {
            var apiResult = new ApiResult();
            try
            {
                if (domainTableBLL.UpdateDomain(updateDomainReq.DId, updateDomainReq.Domain, updateDomainReq.Status, updateDomainReq.Sort, authorizeBLL.GetAuthData().UserName) > 0)
                {
                    apiResult.ErrorCode = 0;
                    apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
                }
                else
                {
                    apiResult.ErrorCode = 9;
                    apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                }
            }
            catch (MySqlException ex)
            {

                if (ex.Number == 1062) //匯入資料重複
                {
                    apiResult.ErrorCode = ex.Number;
                    apiResult.Message = "Duplicate data";
                }
                else
                {
                    apiResult.ErrorCode = ex.Number;
                    apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 91;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

    }
}
