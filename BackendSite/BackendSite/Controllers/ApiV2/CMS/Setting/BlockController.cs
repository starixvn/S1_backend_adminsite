using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BlockController : ControllerBase
    {
        #region Fields
        private readonly BlockBLL _blockBLL;
        private readonly CustInfoBLL _custInfoBll;
        private readonly NewAuthorizeBLL _newAuthorizeBLL;
        #endregion

        #region Constructors
        public BlockController(IServiceProvider provider)
        {
            _blockBLL = provider.GetRequiredService<BlockBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _newAuthorizeBLL = provider.GetRequiredService<NewAuthorizeBLL>();
        }
        #endregion

        #region API Model
        public class GetBlockBankAccRequest
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
        }
        public class BlockBankAccRequest
        {
            public int SysId { get; set; }
            public string AccountNum { get; set; }
        }

        public class GetBlockCryptoWalletRequest
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            public string Address { get; set; }
            public string UserName { get; set; }
            public int Status { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set
                {
                    _startDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set
                {
                    _endDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
                }
            }
            public IEnumerable<int> CurrencyId { get; set; }
            public IEnumerable<int> CryptoType { get; set; }
            public IEnumerable<string> NetWork { get; set; }
        }

        public class BlockCryptoWalletRequest
        {
            public string Address { get; set; }
        }
        #endregion

        #region Action
        /// <summary>
        /// Block/GetBlockIPList 取得被Block的IP List
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiListResult<BlockIP>> GetBlockIPList(GetBlockIPListRequest request)
        {
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var data = await _blockBLL.GetBlockIPList(request.IP, startTime, endTime, request.PageSize, request.PageNumber);
            return new ApiListResult<BlockIP>
            {
                Data = data.BlockIP,
                ErrorCode = (int)ErrorCodeEnum.Success,
                TotalPages = data.TotalPages,
                RowCount = data.RowCount,
                PageNumber = data.PageNumber
            };
        }
        /// <summary>
        /// Block/SetBlockIP 新增Block的IP
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<ApiResult> SetBlockIP(ApiRequestWithLog<SetBlockIPRequest> request)
        {
            var result = await _blockBLL.SetBlockIP(new ParamsWithLog<SetBlockIPRequest>(request, User.Identity.Name));
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Insert Success."
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Insert Fail."
                };
            }
        }

        /// <summary>
        /// Block/DeleteBlockIP 刪除Block的IP
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<ApiResult> DeleteBlockIP(ApiRequestWithLog<BlockIPRequest> request)
        {
            var result = await _blockBLL.DeleteBlockIP(new ParamsWithLog<BlockIPRequest>(request, User.Identity.Name));
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Delete Success."
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Delete Fail."
                };
            }
        }

        /// <summary>
        /// Block/GetBlockBankAcc 取得BlockBankAccount List
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<BlockBankAcc> GetBlockBankAcc(GetBlockBankAccRequest request)
        {
            var blockBankAcc = _blockBLL.GetBlockBankAcc(request.StartDate.DateTime, request.EndDate.DateTime, request.BlockStatus, request.AccountNum, request.CurrencyId, request.UserName);
            return new ApiListResult<BlockBankAcc>
            {
                Data = blockBankAcc,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Block/GetBlockBankAccList 取得BankAccList
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<BlockList> GetBlockBankAccList(BlockBankAccRequest request)
        {
            var blockBankList = _blockBLL.GetBlockBankAccList(request.SysId, request.AccountNum);
            return new ApiListResult<BlockList>
            {
                Data = blockBankList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Block/GetBlockBankAccBlockList 針對Acoount做lock OR unlock
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult BlockAccountSet(ApiRequestWithLog<BlockAccountSetRequest> request)
        {
            var result = _blockBLL.BlockAccountSet(new ParamsWithLog<BlockAccountSetRequest>(request, User.Identity.Name));
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
        /// Block/GetIPUserList 取得某個IP曾經有哪些user使用過
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<IPUser> GetIPUserList(BlockIPRequest request)
        {
            var IPUserList = _blockBLL.GetIPUserList(request.IP);
            return new ApiListResult<IPUser>
            {
                Data = IPUserList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Block/GetDeviceUserList 取得相同DeviceId關聯帳號
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<DeviceUser>>> GetDeviceUserList(GetDeviceUserListRequest request)
        {
            var customer = _custInfoBll.GetCustInfoWithCache(request.CustId);

            var deviceUserList = await _blockBLL.GetDeviceUserList(customer.SiteId, request.DeviceId);
            return new ApiResult<IEnumerable<DeviceUser>>
            {
                Data = deviceUserList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        /// <summary>
        /// FPS Block/GetDeviceUserList 取得相同DeviceId關聯帳號
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<DeviceUser>>> GetFPSDeviceUserList(GetDeviceUserListRequest request)
        {
            var customer = _custInfoBll.GetCustInfoWithCache(request.CustId);

            var deviceUserList = await _blockBLL.GetFPSDeviceUserList(customer.SiteId, request.DeviceId);
            return new ApiResult<IEnumerable<DeviceUser>>
            {
                Data = deviceUserList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Block/ReleaseAccount 解除CustBankDetail跟user的連結
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult ReleaseAccount(ApiRequestWithLog<ReleaseAccountRequest> request)
        {
            var result = _blockBLL.ReleaseAccount(new ParamsWithLog<ReleaseAccountRequest>(request, User.Identity.Name));
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
        /// Block/GetBlockBankAcc 取得BlockBankAccount List
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 142, 1 })]
        public ApiListResult<BlockCryptoWallet> GetBlockCryptoWallet(GetBlockCryptoWalletRequest request)
        {
            var siteId = _newAuthorizeBLL.CheckBrand();
            var custBlockCryptoWallet = _blockBLL.GetBlockCryptoWallet(request.StartDate.DateTime, request.EndDate.DateTime, request.Status, request.Address, request.CurrencyId, request.UserName, request.CryptoType, request.NetWork, siteId);
            return new ApiListResult<BlockCryptoWallet>
            {
                Data = custBlockCryptoWallet,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiListResult<BlockCryptoWalletList> CryptoWalletGetByAddress(BlockCryptoWalletRequest request)
        {
            var siteId = _newAuthorizeBLL.CheckBrand();
            var blockCryptoWalletList = _blockBLL.GetBlockCryptoWalletList(request.Address).ToList();
            blockCryptoWalletList.RemoveAll(x => x.SiteId != siteId);
            return new ApiListResult<BlockCryptoWalletList>
            {
                Data = blockCryptoWalletList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        [HttpPost]
        public ApiResult BlockCryptoWalletSet(ApiRequestWithLog<BlockCryptoWalletSetRequest> request)
        {
            var result = _blockBLL.BlockCryptoWalletSet(new ParamsWithLog<BlockCryptoWalletSetRequest>(request, User.Identity.Name));
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

        [HttpPost]
        public ApiResult ReleaseCryptoWallet(ApiRequestWithLog<ReleaseCryptoWalletRequest> request)
        {
            var result = _blockBLL.ReleaseCryptoWallet(new ParamsWithLog<ReleaseCryptoWalletRequest>(request, User.Identity.Name));
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
        #endregion

    }
}
