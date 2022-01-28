using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.SystemFunc
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class EnumController : ControllerBase
    {

        public class GetEnumTypeReqest
        {
            public string EnumName { set; get; }
        }

        public class EnumTypeModel
        {
            public string Name { set; get; }
            public int Value { set; get; }
        }

        public class EnumTypeWithStringModel
        {
            public string Name { set; get; }
            public string Value { set; get; }
        }

        /// <summary>
        /// Enum/GetEnumType 取得列舉清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// 
        /// Enum List：
        /// 
        /// Deposit Request / Withdrawal Request For Frontend Use - AmountTypeEnum
        /// 
        /// Adjustment Report For Frontend Use - AdjustTypeEnum
        /// 
        /// Customer - IsKycEnum、IsRiskEnum、ActStatusEnum
        /// 
        /// CustIovation - IovationEnum
        /// 
        /// CustProfile - PhoneVerifiedEnum、EmailVerifiedEnum
        /// 
        /// DepositInfo - DepositTypeEnum
        /// 
        /// TpBetRecord - SettlementTypeEnum
        /// 
        /// AdjustWithdrawalLimitInfo - AdjustWithdrawalLimitInfoAdjustTypeEnum、AdjustWithdrawalLimitInfoStatusEnum
        /// 
        /// Promotion - PromotionTypeEnum、PromotionStatusEnum、PromotionPeriodTypeEnum
        ///             PromotionDeviceTypeEnum、PromotionActionEnum 、WalletTypeEnum
        ///             
        /// PromotionCashBackSetting - PromotionCashBackSettingStatusEnum
        /// 
        /// PromotionRebate - PromotionRebateStatusEnum
        /// 
        /// PromotionSpecific - BonusTypeEnum、PromotionSpecificStatusEnum
        /// </remarks>
        /// <param name="reqest">參數</param>
        /// <returns>Data 結果</returns>
        //有新舊API，這支是新API，另一支新的API=api/Common/GetEnumType
        [HttpPost]
        public ApiResult GetEnumType(GetEnumTypeReqest reqest)
        {
            var apiResult = new ApiResult<List<EnumTypeModel>>();
            var result = new List<EnumTypeModel>();
            try
            {
                if (reqest.EnumName == "IovationEnum")
                {
                    return new ApiResult<List<EnumTypeWithStringModel>>()
                    {
                        Data = GetIovationEnumData(),
                        ErrorCode = (int)ErrorCodeEnum.Success
                    };
                };

                var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
                var getType = allTypes.FirstOrDefault(t => string.Equals(t.Name, reqest.EnumName, StringComparison.OrdinalIgnoreCase));
                if (getType != null)
                {
                    if (getType.Namespace == "BackendSite.Service.Model.enums" && getType.IsEnum)
                    {
                        foreach (var enumValue in Enum.GetValues(getType))
                        {
                            result.Add(new EnumTypeModel
                            {
                                Name = Enum.GetName(getType, enumValue),
                                Value = (int)enumValue
                            });
                        }
                        apiResult.Data = result;
                        apiResult.ErrorCode = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }

        #region Private Methods

        private List<EnumTypeWithStringModel> GetIovationEnumData()
        {
            var result = new List<EnumTypeWithStringModel>();

            result.Add(new EnumTypeWithStringModel
            {
                Name = "Allow",
                Value = "A"
            });

            result.Add(new EnumTypeWithStringModel
            {
                Name = "Deny",
                Value = "D"
            });

            result.Add(new EnumTypeWithStringModel
            {
                Name = "Review",
                Value = "R"
            });

            return result;
        }

        #endregion
    }
}
