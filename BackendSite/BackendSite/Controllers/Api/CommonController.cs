using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class CommonController
    {
        private readonly ServerInfoBLL serverInfoBLL;

        public CommonController(ServerInfoBLL serverInfoBLL)
        {
            this.serverInfoBLL = serverInfoBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<ServerInfo>> AdminUserSiteList()
        {
            var apiResult = new ApiResult<IEnumerable<ServerInfo>>();
            try
            {
                apiResult.Data = serverInfoBLL.GetSiteListByLoginUser();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<List<dynamic>> GetEnumType([FromQuery] string enumName)
        {
            var apiResult = new ApiResult<List<dynamic>>();
            List<dynamic> result = new List<dynamic>();
            try
            {
                var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
                var getType = allTypes.Where(t => string.Equals(t.Name, enumName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
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
