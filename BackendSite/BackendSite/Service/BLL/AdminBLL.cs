using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.AdminMenu;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class AdminBLL
    {
        private readonly Tools tools;
        private readonly AdminService adminService;
        private readonly AdminUserService adminUserService;
        private readonly ServerInfoService serverInfoService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AdminBLL(Tools tools,
            AdminService adminService,
            ServerInfoService serverInfoService,
            AdminUserService adminUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.tools = tools;
            this.adminService = adminService;
            this.serverInfoService = serverInfoService;
            this.adminUserService = adminUserService;
            this.httpContextAccessor = httpContextAccessor;
        }

        //暫無使用
        //public bool AddAccount(AdminUser adminUser)
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    adminUser.Userpwd = tools.MD5Encrypt(adminUser.Userpwd);
        //    return adminService.AddAccount(serverInfo.SiteId, adminUser);
        //}

        //暫無使用
        //public IEnumerable<AdminUser> AccountList()
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    return adminService.AccountList(serverInfo.SiteId);
        //}

        //暫無使用
        //public AdminUser GetAccount(int userId)
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    return adminService.GetAccount(serverInfo.SiteId, userId);
        //}

        //暫無使用
        //public bool EditAccount(AdminUser adminUser)
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    return adminService.EditAccount(serverInfo.SiteId, adminUser);
        //}

        //暫無使用
        //public bool ChangePwdAccount(AdminUser adminUser)
        //{
        //    var serverInfo = serverInfoService.GetModel();
        //    if (string.IsNullOrWhiteSpace(adminUser.Userpwd))
        //        adminUser.Userpwd = tools.MD5Encrypt("123456");
        //    else
        //        adminUser.Userpwd = tools.MD5Encrypt(adminUser.Userpwd);
        //    return adminService.ChangePwdAccount(serverInfo.SiteId, adminUser);
        //}

        public AdminUser GetUser(int userId)
        {
            return adminUserService.GetUser(userId);
        }

        public ErrorCodeEnum ChangePwd(ChangePwd changePwd)
        {
            string Md5Userpwd = null;
            var serverInfo = serverInfoService.GetModel();
            var userId = 0;
            int.TryParse(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier).Value, out userId);
            changePwd.OldPwd = tools.MD5Encrypt(changePwd.OldPwd);
            changePwd.NewPwd = tools.MD5Encrypt(changePwd.NewPwd);

            if (adminService.ChangePwd(serverInfo.SiteId, userId, changePwd, out Md5Userpwd))
            {
                return ErrorCodeEnum.Success;
            }
            else
            {
                if (Md5Userpwd != changePwd.OldPwd)
                    return ErrorCodeEnum.OldpwdNotMatch;
                else
                    return ErrorCodeEnum.Fail;
            }
        }
        public SpecialPasswordManagement GetSpecialPasswordData(int sitId)
        {
            return adminService.GetSpecialPasswordData(sitId);
        }

        public int SetSpecialPasswordData(int SiteId, string Password, string Operator)
        {
            return adminService.SetSpecialPasswordData(SiteId, Password, Operator);
        }

        #region New AdminSite Menu Structure

        public async Task<List<AdminMenuModel>> GetAllAdminMenuAsync(string lang)
        {
            var moduleList = await adminService.GetAllAdminModule();
            var functionList = await adminService.GetAllAdminFunction();
            var permissionList = await adminService.GetAllAdminPermission();

            if (!string.IsNullOrWhiteSpace(lang))
            {
                var moduleMappingList = ProcessTranslation(lang, moduleList.Select(x => new MappingTranslationModel { Id = x.Id, Name = x.Name, Translation = x.Translation }));
                moduleList = moduleList.Select(c => { c.Name = moduleMappingList[c.Id]; return c; });
                var functionMappingList = ProcessTranslation(lang, functionList.Select(x => new MappingTranslationModel { Id = x.Id, Name = x.Name, Translation = x.Translation }));
                functionList = functionList.Select(c => { c.Name = functionMappingList[c.Id]; return c; });
                var permissionMappingList = ProcessTranslation(lang, permissionList.Select(x => new MappingTranslationModel { Id = x.Id, Name = x.Name, Translation = x.Translation }));
                permissionList = permissionList.Select(c => { c.Name = permissionMappingList[c.Id]; return c; });
            }

            var heirIdList = moduleList.Where(x => x.Layer != 0).Select(x => x.HeirId).Distinct();
            var result = await GetAllMenu(0, heirIdList, moduleList, functionList, permissionList);

            return result;
        }

        private Dictionary<int, string> ProcessTranslation(string lang, IEnumerable<MappingTranslationModel> mappingList)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in mappingList)
            {
                if (!string.IsNullOrWhiteSpace(item.Translation))
                {
                    var langList = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Translation);
                    if (langList.ContainsKey(lang))
                    {
                        result[item.Id] = langList[lang] ?? item.Name;
                        continue;
                    }
                }
                result[item.Id] = item.Name;
            }
            return result;
        }

        private async Task<List<AdminMenuModel>> GetAllMenu(int moduleIId,
            IEnumerable<int> heirIdList,
            IEnumerable<AdminModuleDto> moduleList,
            IEnumerable<AdminFunctionDto> functionList,
            IEnumerable<AdminPermissionDto> permissionList)
        {
            var result = new List<AdminMenuModel>();
            var assignModuleList = moduleList.Where(x => x.HeirId == moduleIId);

            foreach (var moduleData in assignModuleList)
            {
                var FunctionDatas = new List<AdminMenuFuncModel>();
                var SubLayerDatas = new List<AdminMenuModel>();

                var assignFuncDatas = functionList.Where(x => x.ModuleId == moduleData.Id);

                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<AdminPermissionModel>();
                    var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSettingModel>(funcData.AdditionalSetting);
                    foreach (var permission in assignFuncAdditionalSetting.Permission)
                    {
                        var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                        permissionData.Add(new AdminPermissionModel() { PermissionId = permission, Name = permissionName, CanUse = true });
                    }
                    FunctionDatas.Add(new AdminMenuFuncModel()
                    {
                        FunctionId = funcData.Id,
                        Name = funcData.Name,
                        Permission = permissionData
                    });
                }

                if (heirIdList.Contains(moduleData.Id))
                {
                    var subLayerDatas = await GetAllMenu(moduleData.Id, heirIdList, moduleList, functionList, permissionList);
                    SubLayerDatas.AddRange(subLayerDatas);
                }

                result.Add(new AdminMenuModel()
                {
                    ModuleId = moduleData.Id,
                    Name = moduleData.Name,
                    FunctionDatas = FunctionDatas,
                    SubLayerDatas = SubLayerDatas
                });
            }

            return result;
        }

        #endregion
    }
}
