using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WithdrawalController : ControllerBase
    {
        private readonly WithdrawalBLL withdrawalBLL;
        public WithdrawalController(WithdrawalBLL withdrawalBLL)
        {
            this.withdrawalBLL = withdrawalBLL;
        }

        public class WithdrawalListReq
        {
            public int PageNumber { get; set; }
            public string WithdrawalId { get; set; }
            public string UserName { get; set; }
            public int? withdrawalType { get; set; }
            public int? CurrencyId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
            public int? status { get; set; }
        }

        [HttpPost]
        public ApiListResult<WithdrawalRequest> GetWithdrawalRequestList(WithdrawalListReq withdrawalListReq)
        {
            var apiListResult = new ApiListResult<WithdrawalRequest>();
            int pageNumber = withdrawalListReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var withdrawalRequestList = withdrawalBLL.GetWithdrawalRequestList(withdrawalListReq.withdrawalType, withdrawalListReq.startDate, withdrawalListReq.endDate, withdrawalListReq.WithdrawalId, withdrawalListReq.UserName, withdrawalListReq.CurrencyId, withdrawalListReq.status, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = withdrawalRequestList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }
    }
}