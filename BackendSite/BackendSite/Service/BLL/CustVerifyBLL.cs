using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BackendSite.Service.BLL
{
    public class CustVerifyBLL
    {

        private readonly Common _common;
        private readonly CustInfoBLL _custInfoBll;

        public CustVerifyBLL(IServiceProvider provider)
        {
            _common = provider.GetRequiredService<Common>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
        }

        /// <summary>
        /// 客戶驗證
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userName"></param>
        /// <param name="sabaId"></param>
        /// <param name="custInfo"></param>
        /// <returns></returns>
        public bool CustVerify(int siteId, string userName, string sabaId, out CustInfoDetail custInfo)
        {

            if (string.IsNullOrWhiteSpace(sabaId))
                sabaId = null;
            if (string.IsNullOrWhiteSpace(userName))
                userName = null;

            var custUsername = string.IsNullOrEmpty(userName) ? null : userName;
            var custId = (string.IsNullOrEmpty(sabaId)) ? null : _common.TpIdToCustId(sabaId);

            custInfo = _custInfoBll.GetCustInfoDetails(custId, custUsername, siteId);

            if (custInfo == null)
            {
                return false;
            }

            return true;
        }

    }
}
