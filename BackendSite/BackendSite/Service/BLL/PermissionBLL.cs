using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Service.BLL
{
    public class PermissionBLL
    {
        private readonly AdminGroupService adminGroupService;
        private readonly PermissionService permissionService;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly AdminUserService adminUserService;
        private readonly ServerInfoService serverInfoService;

        public PermissionBLL(PermissionService permissionService, AdminGroupService adminGroupService, AuthorizeBLL authorizeBLL, AdminUserService adminUserService, ServerInfoService serverInfoService)
        {
            this.permissionService = permissionService;
            this.adminGroupService = adminGroupService;
            this.authorizeBLL = authorizeBLL;
            this.adminUserService = adminUserService;
            this.serverInfoService = serverInfoService;
        }

        public IEnumerable<FuncPermissionModel> GetPermissionJSONData()
        {
            var result = JsonConvert.DeserializeObject<IEnumerable<FuncPermissionModel>>(permissionService.GetPermissionJSONData());
            return result;
        }

        public IEnumerable<GroupPermisison> GetAllPermission()
        {
            var result = new List<GroupPermisison>();

            var permissionDatas = GetPermissionJSONData();
            foreach (var func in permissionDatas)
            {
                foreach (var funcKeyItem in func.Settings.Where(funcKeyItem => funcKeyItem != null))
                {
                    if (funcKeyItem.SubFuncKeys != null)
                    {
                        var subFuncKeyDatas = JsonConvert.SerializeObject(funcKeyItem.SubFuncKeys.Select(x => x.FuncKey));
                        result.Add(new GroupPermisison { GroupSEQ = 0, FuncKey = funcKeyItem.FuncKey, SubFuncKeys = subFuncKeyDatas });
                    }
                    else
                    {
                        result.Add(new GroupPermisison
                        {
                            GroupSEQ = 0,
                            FuncKey = funcKeyItem.FuncKey,
                            SubFuncKeys = null
                        });
                    }
                }
            }
            return result;
        }

        public IEnumerable<GroupPermisison> GetUserPermission(int GroupSEQ)
        {
            var permission = adminGroupService.GetGroupPermissions(GroupSEQ);
            return permission;
        }

        public GroupPermisison GetUserPermission(int GroupSEQ, string FuncKey)
        {
            var permission = adminGroupService.GetGroupPermissions(GroupSEQ)?.FirstOrDefault(x => string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase));
            return permission;
        }

        public IEnumerable<FuncPermissionModel> GetGroupPermissionModel(int GroupSEQ)
        {
            var result = GetPermissionJSONData();
            var permissions = adminGroupService.GetGroupPermissions(GroupSEQ);
            if (permissions.Any())
            {
                foreach (var item in result)
                {
                    item.Settings = SetPermissions(item.Settings, permissions, null);
                }
            }

            return result;
        }

        private List<FuncPermissionSettings> SetPermissions(IEnumerable<FuncPermissionSettings> Settings, IEnumerable<GroupPermisison> GroupPermissions, IEnumerable<UserPermission> UserPermissions)
        {
            var result = new List<FuncPermissionSettings>();
            foreach (var setting in Settings)
            {
                result.Add(SetPermissions(setting, GroupPermissions, UserPermissions));
            }

            return result;
        }

        private FuncPermissionSettings SetPermissions(FuncPermissionSettings Setting, IEnumerable<GroupPermisison> GroupPermissions, IEnumerable<UserPermission> UserPermissions)
        {
            if (Setting.FuncKeys != null && Setting.FuncKeys.Any())
            {
                var functionkeys = new List<FuncPermissionSettings>();
                foreach (var funckeys in Setting.FuncKeys)
                {
                    functionkeys.Add(SetPermissions(funckeys, GroupPermissions, UserPermissions));
                }
                Setting.Enable = functionkeys.Any(x => x.Enable);
                Setting.FuncKeys = functionkeys;
                Setting.IsGroupSetting = functionkeys.Any(x => x.IsGroupSetting);
            }
            else
            {
                var funcByUser = UserPermissions?.SingleOrDefault(i => i.FuncKey == Setting.FuncKey);
                var funcByGroup = GroupPermissions?.SingleOrDefault(x => x.FuncKey == Setting.FuncKey);

                Setting = SetGroupPermission(Setting, funcByGroup);

                if (Setting.InputBoxType == "radio" && funcByGroup != null && funcByUser != null && funcByUser.SubFuncKeys != funcByGroup.SubFuncKeys)
                {

                }
                else
                {
                    Setting = SetUserPermission(Setting, funcByUser);
                }
            }

            return Setting;
        }

        private FuncPermissionSettings SetGroupPermission(FuncPermissionSettings Setting, GroupPermisison GroupPermission)
        {
            if (GroupPermission != null)
            {
                Setting.IsGroupSetting = true;
                Setting.Enable = true;
                if (GroupPermission.SubFuncKeys != null && Setting.InputBoxType != null && Setting.SubFuncKeys.Any())
                {
                    var userSubPermission = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(GroupPermission.SubFuncKeys);
                    if (userSubPermission != null && userSubPermission.Any())
                    {
                        var checkedIndexItemList = new List<int>();
                        foreach (var subFuncKey in userSubPermission)
                        {
                            var idx = Setting.SubFuncKeys.FindIndex(i => i.FuncKey.ToUpper() == subFuncKey.ToUpper());
                            if (idx >= 0) checkedIndexItemList.Add(idx);
                        }

                        Setting.GroupCheckedIndex = checkedIndexItemList.ToArray<int>();
                    }
                }
            }

            return Setting;
        }

        private FuncPermissionSettings SetUserPermission(FuncPermissionSettings Setting, UserPermission UserPermission)
        {
            if (UserPermission != null)
            {
                Setting.Enable = true;
                if (Setting.Enable && UserPermission.SubFuncKeys != null && Setting.InputBoxType != null)
                {
                    var userSubPermission = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(UserPermission.SubFuncKeys);
                    if (userSubPermission != null && userSubPermission.Any())
                    {
                        var checkedIndexItemList = new List<int>();
                        foreach (var subFuncKey in userSubPermission)
                        {
                            var idx = Setting.SubFuncKeys.FindIndex(i => i.FuncKey.ToUpper() == subFuncKey.ToUpper());
                            if (idx >= 0) checkedIndexItemList.Add(idx);
                        }

                        Setting.UserCheckedIndex = checkedIndexItemList.ToArray<int>();
                    }
                }
            }

            return Setting;
        }

        public bool CheckPermission(string FuncKey, string SubFuncKey)
        {
            var authData = authorizeBLL.GetAuthData();
            var loginUser = adminUserService.GetUser(authData.UserId);

            var result = false;
            if (loginUser.IsSuper)
            {
                result = true;
            }
            else
            {
                if (loginUser.GroupSEQ == null)
                {
                    result = false;
                }
                else
                {
                    var permissions = GetUserPermission((int)loginUser.GroupSEQ);
                    if (permissions != null && permissions.Any())
                    {
                        var funcItem = permissions.FirstOrDefault(x => string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase));
                        var subItem = funcItem.SubFuncKeys == null ? new string[] { } : Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(funcItem.SubFuncKeys);

                        if (!string.IsNullOrEmpty(SubFuncKey))
                        {
                            if (subItem.Any())
                            {
                                result = subItem.Any(s => string.Equals(s, SubFuncKey, StringComparison.OrdinalIgnoreCase));
                            }
                        }
                        else
                        {
                            result = funcItem != null;
                        }
                    }
                }
            }

            return result;
        }

        public bool CheckPermission(int GroupSEQ, string FuncKey, string SubFuncKey)
        {
            var authData = authorizeBLL.GetAuthData();
            var loginUser = adminUserService.GetUser(authData.UserId);

            var result = false;
            var permissions = GetUserPermission(GroupSEQ);
            if (permissions != null && permissions.Any())
            {
                var funcItem = permissions.FirstOrDefault(x => string.Equals(x.FuncKey, FuncKey, StringComparison.OrdinalIgnoreCase));
                var subItem = funcItem.SubFuncKeys == null ? new string[] { } : Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(funcItem.SubFuncKeys);

                if (!string.IsNullOrEmpty(SubFuncKey))
                {
                    if (subItem.Any())
                    {
                        result = subItem.Any(s => string.Equals(s, SubFuncKey, StringComparison.OrdinalIgnoreCase));
                    }
                }
                else
                {
                    result = funcItem != null;
                }
            }

            return result;
        }

        public IEnumerable<string> GetSubFunckeys(string FuncKey)
        {
            var authData = authorizeBLL.GetAuthData();
            var loginUser = adminUserService.GetUser(authData.UserId);
            IEnumerable<string> result = null;
            if (loginUser.IsSuper)
            {
                var initDatas = GetPermissionJSONData();
                foreach (var func in initDatas)
                {
                    var item = GetFuncItem(func.Settings, FuncKey);
                    if (item != null)
                    {
                        result = item.SubFuncKeys.Select(x => x.FuncKey);
                        break;
                    }
                }
            }
            else if (loginUser.GroupSEQ != null)
            {
                var permissions = GetUserPermission((int)loginUser.GroupSEQ, FuncKey);
                if (permissions != null && permissions.SubFuncKeys != null)
                {
                    result = JsonConvert.DeserializeObject<IEnumerable<string>>(permissions.SubFuncKeys);
                }
            }

            return result;
        }

        private FuncPermissionSettings GetFuncItem(List<FuncPermissionSettings> Settings, string FuncKey)
        {
            FuncPermissionSettings result = null;
            foreach (var func in Settings)
            {
                if (string.Equals(func.FuncKey, FuncKey))
                {
                    result = func;
                    break;
                }
                else if (func.FuncKeys != null && func.FuncKey.Any())
                {
                    return GetFuncItem(func.FuncKeys, FuncKey);
                }
            }

            return result;
        }

        public bool DashBoardAuthorize()
        {
            var result = false;

            var authData = authorizeBLL.GetAuthData();
            var loginUser = adminUserService.GetUser(authData.UserId);

            if (loginUser.IsSuper)
            {
                result = true;
            }
            else if (loginUser.GroupSEQ != null)
            {
                var permissions = GetUserPermission((int)loginUser.GroupSEQ, "Home/DashboardNew");
                if (permissions != null)
                {
                    result = true;
                }
            }

            return result;
        }

        //public NewPermission GetNewPermission(AdminUserV2 loginUser)
        //{
        //    NewPermission result = new NewPermission();
        //    if (loginUser.IsSuper)
        //    {
        //        var userSiteList = serverInfoService.GetSiteList().ToList();
        //        result.CurrencyList = userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;

        //        var allMenu = GetAdminMenuDtoV2().ToList();
        //        var allPermission = GetAdminPermissionDtoV2().ToList();

        //        List<Permission> permissions = new List<Permission>();
        //        var menu = allMenu.Where(x => x.HeirId == 0).Select(y => y).ToList();
        //        menu.ForEach(x =>
        //        {
        //            List<PermissionSettings> settings = new List<PermissionSettings>();
        //            List<FunctionDatas> functionDatas = new List<FunctionDatas>();
        //            allMenu.Where(y => y.HeirId == x.Id).Select(z => z).ToList().ForEach(t =>
        //            {
        //                if (!String.IsNullOrWhiteSpace(t.AdditionalSetting))
        //                {
        //                    var additionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(t.AdditionalSetting);
        //                    if (additionalSetting != null)
        //                    {
        //                        PermissionSettings permissionSettings = new PermissionSettings();
        //                        List<PermissionSubFuncKeys> permissionSubFuncKeysList = new List<PermissionSubFuncKeys>();
        //                        additionalSetting.Permission.ToList().ForEach(u =>
        //                        {
        //                            var permissionName = allPermission.Where(i => i.Id == u).FirstOrDefault().Name;
        //                            PermissionSubFuncKeys permissionSubFuncKeys = new PermissionSubFuncKeys()
        //                            {
        //                                Name = permissionName,
        //                                PermissionId = u
        //                            };
        //                            permissionSubFuncKeysList.Add(permissionSubFuncKeys);
        //                        });
        //                        permissionSettings.SubFuncKeys = permissionSubFuncKeysList;
        //                        permissionSettings.DisPlayKey = t.Name;
        //                        permissionSettings.FunKey = t.Id;
        //                        settings.Add(permissionSettings);
        //                    }
        //                }
        //                else
        //                {
        //                    FunctionDatas functionData = new FunctionDatas();
        //                    List<PermissionSettings> subSettings = new List<PermissionSettings>();
        //                    var subMenu = allMenu.Where(s => s.HeirId == t.Id).Select(d => d).ToList();
        //                    subMenu.ForEach(f =>
        //                    {
        //                        var additionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(f.AdditionalSetting);
        //                        PermissionSettings permissionSettings = new PermissionSettings();
        //                        List<PermissionSubFuncKeys> permissionSubFuncKeysList = new List<PermissionSubFuncKeys>();
        //                        additionalSetting.Permission.ToList().ForEach(u =>
        //                        {
        //                            var permissionName = allPermission.Where(i => i.Id == u).FirstOrDefault().Name;
        //                            PermissionSubFuncKeys permissionSubFuncKeys = new PermissionSubFuncKeys()
        //                            {
        //                                Name = permissionName,
        //                                PermissionId = u
        //                            };
        //                            permissionSubFuncKeysList.Add(permissionSubFuncKeys);
        //                        });
        //                        permissionSettings.SubFuncKeys = permissionSubFuncKeysList;
        //                        permissionSettings.DisPlayKey = t.Name;
        //                        permissionSettings.FunKey = t.Id;
        //                        subSettings.Add(permissionSettings);
        //                    });
        //                    functionData.DisPlayKey = t.Id;
        //                    functionData.Name = t.Name;
        //                    functionData.Settings = subSettings;
        //                    functionDatas.Add(functionData);
        //                }
        //            });

        //            Permission permission = new Permission()
        //            {
        //                DisPlayKey = x.Id,
        //                Name = x.Name,
        //                Settings = settings,
        //                FunctionDatas = functionDatas
        //            };
        //            permissions.Add(permission);
        //        });

        //        result.Permission = permissions;
        //    }
        //    else
        //    {
        //        result.CurrencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.CurrencyList);
        //    }

        //    return result;
        //}
        public IEnumerable<AdminMenuDtoV2> GetAdminMenuDtoV2()
        {
            return permissionService.GetAdminMenuDtoV2();
        }
        public IEnumerable<AdminPermissionDtoV2> GetAdminPermissionDtoV2()
        {
            return permissionService.GetAdminPermissionDtoV2();
        }
        public IEnumerable<AdminRole> GetRolePermission(int role)
        {
            return permissionService.GetRolePermission(role);
        }

        public NewPermission GetNewPermissionV2(AdminUserV2 loginUser)
        {
            NewPermission result = new NewPermission();
            List<Permission> permissionReslut = new List<Permission>();
            var menuList = GetAdminMenuDtoV2().ToList();
            var permissionList = GetAdminPermissionDtoV2().ToList();
            var heirIdList = menuList.Where(x => x.HeirId != 0).Select(x => x.HeirId).Distinct();
            var assignFuncDatas = menuList.Where(x => x.HeirId == 0 && !heirIdList.Contains(x.Id));
            if (loginUser.IsSuper)
            {
                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                    {
                        var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                        foreach (var permission in assignFuncAdditionalSetting.Permission)
                        {
                            var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                            permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = true });
                        }
                    }
                    permissionReslut.Add(new Permission()
                    {
                        ModuleId = null,
                        DisPlayKey = funcData.Id,
                        Name = funcData.Name,
                        Settings = permissionData
                    });
                }
                permissionReslut.AddRange(GetAllMenuV2(0, heirIdList, menuList, permissionList));

                var userSiteList = serverInfoService.GetSiteList().ToList();
                result.CurrencyList = userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;
                result.Permission = permissionReslut;
            }
            else
            {
                var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                var userPermissionList = GetRolePermission(userRole).ToList();

                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    var userPermission = userPermissionList.Find(x => x.MenuId == funcData.Id);
                    if (userPermission != null)
                    {                 
                        if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                        {
                            var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                            foreach (var permission in assignFuncAdditionalSetting.Permission)
                            {
                                //if (userPermission.Permission.ToList().Exists(x=> x == permission))
                                //{
                                //    var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                //    permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = true });
                                //}
                                if (JsonConvert.DeserializeObject<List<int>>(userPermission.Permission).Exists(x => x == permission))
                                {
                                    var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                    permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = true });
                                }
                            }
                        }
                        permissionReslut.Add(new Permission()
                        {
                            ModuleId = null,
                            DisPlayKey = funcData.Id,
                            Name = funcData.Name,
                            Settings = permissionData
                        });
                    }
                }
                permissionReslut.AddRange(GetAllMenuV2(0, heirIdList, menuList, permissionList, userPermissionList));

                result.CurrencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.CurrencyList);
                result.Permission = permissionReslut;
            }
            return result;
        }

        private List<Permission> GetAllMenuV2(int moduleIId,
           IEnumerable<int> heirIdList,
           IEnumerable<AdminMenuDtoV2> menuList,
           IEnumerable<AdminPermissionDtoV2> permissionList,
           IEnumerable<AdminRole> userPermissionList = null)
        {
            var result = new List<Permission>();
            var assignModuleList = menuList.Where(x => x.HeirId == moduleIId);

            foreach (var moduleData in assignModuleList)
            {
                var FunctionDatas = new List<FunctionDatas>();
                var SubLayerDatas = new List<Permission>();

                var assignFuncDatas = menuList.Where(x => x.HeirId == moduleData.Id);

                foreach (var funcData in assignFuncDatas)
                {
                    
                    if(userPermissionList != null)
                    {
                        var userPermission = userPermissionList.ToList().Find(x => x.MenuId == funcData.Id);

                        if(userPermission != null)
                        {
                            var permissionData = new List<PermissionSubFuncKeys>();
                            if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                            {
                                var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                                foreach (var permission in assignFuncAdditionalSetting.Permission)
                                {
                                    //if (userPermission.Permission.ToList().Exists(x=> x == permission))
                                    //{
                                    //    var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                    //    permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = true });
                                    //}
                                    if (JsonConvert.DeserializeObject<List<int>>(userPermission.Permission).Exists(x => x == permission))
                                    {
                                        var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                        permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = true });
                                    }
                                }
                                FunctionDatas.Add(new FunctionDatas()
                                {
                                    DisPlayKey = funcData.Id,
                                    Name = funcData.Name,
                                    Settings = permissionData
                                });
                            }
                            else
                                continue;
                        }
                    }
                    else
                    {
                        var permissionData = new List<PermissionSubFuncKeys>();
                        if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                        {
                            var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                            foreach (var permission in assignFuncAdditionalSetting.Permission)
                            {
                                var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = true });
                            }
                            FunctionDatas.Add(new FunctionDatas()
                            {
                                DisPlayKey = funcData.Id,
                                Name = funcData.Name,
                                Settings = permissionData
                            });
                        }
                        else
                            continue;
                    }             
                }

                if (heirIdList.Contains(moduleData.Id))
                {
                    var subLayerDatas =  GetAllMenuV2(moduleData.Id, heirIdList, menuList, permissionList, userPermissionList);
                    SubLayerDatas.AddRange(subLayerDatas);
                }

                if (FunctionDatas.Any() || SubLayerDatas.Any())
                {
                    result.Add(new Permission()
                    {
                        ModuleId = moduleData.Id,
                        Name = moduleData.Name,
                        FunctionDatas = FunctionDatas,
                        SubLayerDatas = SubLayerDatas
                    });
                }
            }

            return result;
        }
    }
}
