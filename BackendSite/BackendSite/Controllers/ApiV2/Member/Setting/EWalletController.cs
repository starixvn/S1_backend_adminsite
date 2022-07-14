using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class EWalletController : ControllerBase
    {
        #region Fields
        private readonly EWalletBLL _eWalletBLL;
        #endregion

        #region Constructors
        public EWalletController(IServiceProvider provider)
        {
            _eWalletBLL = provider.GetRequiredService<EWalletBLL>();
        }
        #endregion

        #region API Model
        public class GetBlockEWalletAccRequest
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            public string AccountNum { get; set; }
            public string UserName { get; set; }
            public int BlockStatus { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
                }
            }
            public IEnumerable<int> CurrencyId { get; set; }
            public IEnumerable<int> EWalletId { get; set; }
        }
        public class BlockEWalletAccRequest
        {
            public int SysId { get; set; }
            public string AccountNum { get; set; }
        }
        public class BlockEWalletSetRequest : BlockEWalletAccRequest
        {
            public BlockAccountStatus Status { get; set; }
            public string Memo { get; set; }
        }

        public class ReleaseAccountRequest : BlockEWalletAccRequest
        {
            public int CustId { get; set; }
        }
        #endregion

        #region Action
        /// <summary>
        /// EWallet/GetBlockEWalletAcc 取得BlockEWalletAcc
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<BlockEWalletAcc> GetBlockEWalletAcc(GetBlockEWalletAccRequest request)
        {
            var blockEWalletAcc = _eWalletBLL.GetBlockEWalletAcc(request.StartDate.DateTime, request.EndDate.DateTime, request.BlockStatus, request.AccountNum, request.CurrencyId, request.UserName, request.EWalletId);
            return new ApiListResult<BlockEWalletAcc>
            {
                Data = blockEWalletAcc,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// EWallet/GetBlockEWalletList 取得BlockEWalletList
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<EWallet> GetBlockEWalletList(BlockEWalletAccRequest request)
        {
            var blockEWalletList = _eWalletBLL.GetBlockEWalletList(request.SysId, request.AccountNum);
            return new ApiListResult<EWallet>
            {
                Data = blockEWalletList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// EWallet/BlockEWalletSet 針對EWalletAcc做lock OR unlock
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult BlockEWalletSet(BlockEWalletSetRequest request)
        {
            var result = _eWalletBLL.BlockEWalletSet(request.SysId, request.AccountNum, request.Status, User.Identity.Name, request.Memo);
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Update Fail."
                };
            }
        }

        /// <summary>
        /// EWallet/ReleaseAccount 解除CustEWalletDetail跟user的連結
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult ReleaseAccount(ReleaseAccountRequest request)
        {
            var result = _eWalletBLL.ReleaseAccount(request.CustId, request.SysId, request.AccountNum);
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Release Success."
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Release Fail."
                };
            }
        }

        /// <summary>
        /// EWallet/GetEWalletInfo 取得EWalletInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<EWalletInfoSys> GetEWalletInfo()
        {
            var eWalletInfo = _eWalletBLL.GetEWalletInfo();
            return new ApiListResult<EWalletInfoSys>
            {
                Data = eWalletInfo,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion
    }
}
