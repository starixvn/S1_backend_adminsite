using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancial/TransferManage" })]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TransferManageController : Controller
    {
        #region Fields

        private readonly AuthorizeBLL _authorizeBLL;
        private readonly BankFinancialBLL _bankFinancialBLL;
        private readonly BankDetailBLL _bankDetailBLL;
        private readonly ExcelBLL _excelBLL;

        #endregion

        #region Constructors

        public TransferManageController(IConfiguration configuration,
                AuthorizeBLL authorizeBLL,
                BankFinancialBLL bankFinancialBLL,
                BankDetailBLL bankDetailBLL,
                ExcelBLL excelBLL)
        {
            _authorizeBLL = authorizeBLL;
            _bankFinancialBLL = bankFinancialBLL;
            _bankDetailBLL = bankDetailBLL;
            _excelBLL = excelBLL;
        }

        #endregion


        #region API Model

        public class GetTransferInfoReq
        {
            public TransferTypeEnum? TransferType { get; set; }

            public string TransFromDate { get; set; }

            public string TransToDate { get; set; }

            public string FromAccountNo { get; set; }

            public string FromAccountDisplayName { get; set; }

            public string ToAccountNo { get; set; }

            public string ToAccountDisplayName { get; set; }

            public int? Reason { get; set; }

            public string TransferId { get; set; }
        }

        #endregion

        #region Action

        #region Data For Set

        public ApiResult GetTransferAccountList()
        {
            var apiResult = new ApiResult<IEnumerable<BankFinancialTransferAccountDetailModel>>();

            try
            {
                apiResult.Data = _bankFinancialBLL.GetTransferAccountList();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.Message;
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
            }

            return apiResult;
        }

        #endregion

        #endregion
    }
}
