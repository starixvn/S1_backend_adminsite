using System;
using System.Collections.Generic;
using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.BLL
{
    public class ShieldBLL
    {
        #region Fields

        private readonly ShieldService _shieldService;
        private readonly SelLangBLL _selLangBll;

        #endregion

        #region Constructors

        public ShieldBLL(IServiceProvider provider)
        {
            _shieldService = provider.GetRequiredService<ShieldService>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        #endregion
        
        public IEnumerable<ShieldTaggingReport> GetList(ShieldTaggingReportController.GetListReq request ,out int totalPages, out int rowCount)
        {
            return _shieldService.GetList(request,out totalPages, out rowCount);
        }
        
        public IEnumerable<ShieldAssociateAccountResult> GetAssociateAccount(ShieldTaggingReportController.GetAssociateAccountReq request)
        {
            return _shieldService.GetAssociateAccount(request);
        }
    }
}