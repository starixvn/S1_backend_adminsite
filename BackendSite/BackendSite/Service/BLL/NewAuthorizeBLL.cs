using System.Security.Claims;
using BackendSite.ExceptionHandling;
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
        private readonly Tools _tools;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NewAuthorizeBLL(IServiceProvider provider)
        {
            _newAdminUserService = provider.GetRequiredService<NewAdminUserService>();
            _adminLogService = provider.GetRequiredService<AdjustmentLogReportService>();
            _tools = provider.GetRequiredService<Tools>();
            _httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        }

        public ErrorCodeEnum UserLogin(string userName,
                                              string pwd,
                                              string ip,
                                              string fromUrl,
                                              string userAgent,
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

            adminUser = _newAdminUserService.GetAdminUserByUserName(userName);

            if (adminUser == null)
            {
                return ErrorCodeEnum.AccountError;
            }
            else if (adminUser.Userpwd != _tools.MD5Encrypt(pwd))
            {
                return ErrorCodeEnum.PasswordError;
            }
            else if (adminUser.Status != (int)AdminUserStatusEnum.Running)
            {
                return ErrorCodeEnum.AccountLocked;
            }
            else
            {
                return ErrorCodeEnum.Success;
            }
        }

        public AuthData GetAuthData()
        {
            AuthData authData = null;
            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                int.TryParse(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier).Value, out var userId);
                int.TryParse(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Thumbprint)?.Value, out var siteId);

                authData = new AuthData
                {
                    UserId = userId,
                    UserName = _httpContextAccessor.HttpContext.User.Identity.Name,
                    SiteId = siteId
                };
            }

            return authData;
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
