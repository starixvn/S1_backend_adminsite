using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.ApiV2.System
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EnumController : ControllerBase
    {

        public class GetEnumTypeReqest
        {
            public string EnumName { set; get; }
        }


        /// <summary>
        /// Enum/GetEnumType 取得列舉清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Global Func.
        /// </remarks>
        /// <param name="reqest">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        public ApiResult<List<dynamic>> GetEnumType(GetEnumTypeReqest reqest)
        {
            var apiResult = new ApiResult<List<dynamic>>();
            var result = new List<dynamic>();
            try
            {
                var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
                var getType = allTypes.FirstOrDefault(t => string.Equals(t.Name, reqest.EnumName, StringComparison.OrdinalIgnoreCase));
                if (getType != null)
                {
                    if (getType.Namespace == "BackendSite.Service.Model.enums" && getType.IsEnum)
                    {
                        foreach (var enumValue in Enum.GetValues(getType))
                        {
                            result.Add(new { name = Enum.GetName(getType, enumValue), value = (int)enumValue });
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
    }
}
