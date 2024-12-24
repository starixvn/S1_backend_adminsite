using System.Security.Claims;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class NewAuthorizeBLL
    {
        private readonly NewAdminUserService _newAdminUserService;
        private readonly AdjustmentLogReportService _adminLogService;
        private readonly ConfigBLL _configBll;
        private readonly Tools _tools;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NewAuthorizeBLL> _logger;

        public NewAuthorizeBLL(IServiceProvider provider)
        {
            _newAdminUserService = provider.GetRequiredService<NewAdminUserService>();
            _adminLogService = provider.GetRequiredService<AdjustmentLogReportService>();
            _configBll = provider.GetRequiredService<ConfigBLL>();
            _tools = provider.GetRequiredService<Tools>();
            _httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            _logger = provider.GetRequiredService<ILogger<NewAuthorizeBLL>>();
        }

        public ErrorCodeEnum UserLogin(string userName,
                                              string pwd,
                                              string ip,
                                              string fromUrl,
                                              string userAgent,
                                              AdminUserTypeEnum type,
                                              ref NewAdminUser adminUser)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return ErrorCodeEnum.AccountInvalid;
            }
            
            if (string.IsNullOrEmpty(pwd))
            {
                return ErrorCodeEnum.PasswordInvalid;
            }

            // 檢查 userType => domain
            /*var domains = _configBll
                .GetRawConfigValues<Dictionary<AdminUserTypeEnum, IEnumerable<string>>>("AdminTypeDomain")
                .First()
                .FirstOrDefault(x => x.Key == type);

            if (!domains.Value.Contains(fromUrl))
            {
                _logger.LogError($"fromUrl={fromUrl}, type={type}, has no valid domain.");
                return ErrorCodeEnum.AccountError;
            }*/

            adminUser = _newAdminUserService
                .GetAdminUserByUserName(userName)
                .FirstOrDefault();
                //.FirstOrDefault(x => x.UserType == type);

            if (adminUser == null)
            {
                return ErrorCodeEnum.AccountError;
            }

            if (adminUser.Userpwd != _tools.MD5Encrypt(pwd))
            {
                return ErrorCodeEnum.PasswordError;
            }

            if (adminUser.Status != (int)AdminUserStatusEnum.Running)
            {
                return ErrorCodeEnum.AccountLocked;
            }

            return ErrorCodeEnum.Success;
        }

        public AuthData GetAuthData()
        {
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                return null;
            
            _ = int.TryParse(_httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier)?.Value, out var userId);
                
            _ = int.TryParse(_httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(m => m.Type == ClaimTypes.Thumbprint)?.Value, out var siteId);
            
            _ = int.TryParse(_httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(m => m.Type == ClaimTypes.Role)?.Value, out var userType);
            
            _ = int.TryParse(_httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(m => m.Type == ClaimTypes.GroupSid)?.Value, out var refId);

            return new AuthData
            {
                UserId = userId,
                UserName = _httpContextAccessor.HttpContext.User.Identity.Name,
                SiteId = siteId,
                UserType = (AdminUserTypeEnum)userType,
                RefId = refId
            };
        }

        public int SetLoginTime(int userId)
        {
            return _newAdminUserService.SetLoginTime(userId);
        }

        public List<AssignBrand> GetAssignSiteListV2(int userId)
        {
            return _newAdminUserService.GetAssignSiteListV2(userId);
        }

        public int CheckBrand()
        {
            _ = int.TryParse(_httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(m => m.Type == ClaimTypes.Thumbprint)?.Value, out var brand);
            
            return brand;
        }

        public IEnumerable<SpecialAdminUser> GetSpecialAdminUserList(int refId, AdminUserTypeEnum userType)
        {
            return _newAdminUserService.GetSpecialAdminUserList(refId, userType);
        }
        
        public SpecialAdminUser GetSpecialAdminUserById(int refId, AdminUserTypeEnum userType, int userId)
        {
            var result = _newAdminUserService
                .GetSpecialAdminUserList(refId, userType, userId)
                .FirstOrDefault();
            
            if (result == null)
                throw new AdminException($"No user found. userId={userId}, userType={userType}, refId={refId}");
            
            return result;
        }

        public bool SetSpecialAdminUser(SetSpecialAdminUserParams input)
        {
            var success = false;
            var isCreate = input.UserId == 0;
            input.Userpwd = string.IsNullOrEmpty(input.Userpwd)
                ? string.Empty : _tools.MD5Encrypt(input.Userpwd);
            
            var logParams = new CreateAdminAdjustmentInfoLogInput
            {
                SiteId = 0,
                CurrencyId = 0,
                Operator = input.Operator,
                ActionType = isCreate ? ActionType.Create : ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.SetSpecialAdminUser,
                AfterDetailInfo = JsonConvert.SerializeObject(new
                {
                    input.RefId,
                    UserType = input.UserType.ToString(),
                    input.UserName,
                    input.Userpwd,
                    input.SiteIds,
                    input.CurrencyIds,
                    input.GroupIds
                })
            };
            
            if (isCreate)
            {
                (success, var userId) = _newAdminUserService.CreateSpecialAdminUser(input);
            }
            else
            {
                // todo: set user
                
                var oldData = GetSpecialAdminUserById(input.RefId, input.UserType, input.UserId);
                logParams.BeforeDetailInfo = JsonConvert.SerializeObject(new
                {
                    input.RefId,
                    UserType = input.UserType.ToString(),
                    oldData.SiteIds,
                    oldData.CurrencyIds,
                    oldData.GroupIds
                });

                success = true;
            }

            if (success)
            {
                _ = _adminLogService.CreateAdminAdjustmentInfoLog(logParams);
            }

            return success;
        }
    }
}
