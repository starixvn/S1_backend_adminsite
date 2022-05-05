using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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

        #region New Admin-Administration

        #region RolePermission
        public IEnumerable<RolePermission> GetRolePermission(int role)
        {
            return permissionService.GetRolePermission(role);
        }
        public IEnumerable<UserAllPremission> GetUserAllPremission(int? role, int userId, int siteId)
        {
            List<UserAllPremission> userAllPremission = new List<UserAllPremission>();
            if (role != null)
            {
                var rolePermission = permissionService.GetRolePermission(role.Value).ToList();
                rolePermission.ForEach(x =>
                {
                    var permission = JsonConvert.DeserializeObject<List<int>>(x.Permission);
                    userAllPremission.Add(new UserAllPremission() { MenuId = x.MenuId, Permission = permission });
                });
            }

            var Permission = permissionService.GetUserPermissionData(userId, siteId).ToList();
            Permission.ForEach(x =>
            {
                var permission = JsonConvert.DeserializeObject<List<int>>(x.Permission);
                if (userAllPremission.Exists(y => y.MenuId == x.MenuId))
                {
                    var tempPremission = userAllPremission.Find(z => z.MenuId == x.MenuId).Permission;
                    permission.ForEach(y =>
                    {
                        if (!tempPremission.Exists(s => s == y))
                        {
                            tempPremission.Add(y);
                        }
                    });
                    userAllPremission.Find(z => z.MenuId == x.MenuId).Permission = tempPremission;
                }
                else
                {
                    userAllPremission.Add(new UserAllPremission() { MenuId = x.MenuId, Permission = permission });
                }
            });

            return userAllPremission;
        }
        public NewPermission GetAddRolePermission(AdminUserV2 loginUser)
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
                            permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = false });
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
                //permissionReslut.AddRange(GetAllRolePermissionMenu(0, heirIdList, menuList, permissionList));
                permissionReslut.AddRange(GerPermission(PermissionTypeEnum.AddRole, 0, heirIdList, menuList, permissionList));
                var userSiteList = serverInfoService.GetSiteList().ToList();
                result.CurrencyList = userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;
                result.Permission = permissionReslut;
            }
            else
            {
                var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                var userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();

                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    var userPermission = userAllPremission.Find(x => x.MenuId == funcData.Id);
                    if (userPermission != null)
                    {
                        if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                        {
                            var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                            foreach (var permission in assignFuncAdditionalSetting.Permission)
                            {
                                if (userPermission.Permission.Exists(x => x == permission))
                                {
                                    var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                    permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = false });
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
                //permissionReslut.AddRange(GetAllRolePermissionMenu(0, heirIdList, menuList, permissionList, userAllPremission));
                permissionReslut.AddRange(GerPermission(PermissionTypeEnum.AddRole, 0, heirIdList, menuList, permissionList, null, userAllPremission));
                result.CurrencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.CurrencyList);
                result.Permission = permissionReslut;
            }
            return result;
        }
        public RoleData GetUpdateRolePermission(int role, AdminUserV2 loginUser)
        {
            RoleData result = new RoleData();
            List<Permission> permissionReslut = new List<Permission>();
            var menuList = GetAdminMenuDtoV2().ToList();
            var permissionList = GetAdminPermissionDtoV2().ToList();
            var heirIdList = menuList.Where(x => x.HeirId != 0).Select(x => x.HeirId).Distinct();
            var assignFuncDatas = menuList.Where(x => x.HeirId == 0 && !heirIdList.Contains(x.Id));
            var rolePermissions = permissionService.GetRolePermission(role).ToList();
            if (loginUser.IsSuper)
            {
                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    var rolePermission = rolePermissions.Find(x => x.MenuId == funcData.Id);
                    if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                    {
                        var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                        foreach (var permission in assignFuncAdditionalSetting.Permission)
                        {
                            var canUse = false;
                            bool roolhasPermission = false;
                            if (rolePermission != null)
                            {
                                roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                            }
                            if (roolhasPermission)
                            {
                                canUse = true;
                            }
                            var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                            permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse });
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
                //permissionReslut.AddRange(GetRolePermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions));
                permissionReslut.AddRange(GerPermission(PermissionTypeEnum.UpdateRole, 0, heirIdList, menuList, permissionList, rolePermissions));
                var userSiteList = serverInfoService.GetSiteList().ToList();
                result.CurrencyList = userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;
                result.Permission = permissionReslut;
            }
            else
            {
                var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                var userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();

                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    var userPermission = userAllPremission.Find(x => x.MenuId == funcData.Id);
                    var rolePermission = rolePermissions.Find(x => x.MenuId == funcData.Id);
                    if (userPermission != null || rolePermission != null)
                    {
                        if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                        {
                            var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                            foreach (var permission in assignFuncAdditionalSetting.Permission)
                            {
                                bool userhasPermission = false;
                                bool roolhasPermission = false;
                                if (userPermission != null)
                                {
                                    userhasPermission = userPermission.Permission.Exists(x => x == permission);
                                }
                                if (rolePermission != null)
                                {
                                    roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                }
                                if (userhasPermission || roolhasPermission)
                                {
                                    var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                    var enable = true;
                                    var canUse = true;
                                    if (!userhasPermission)
                                    {
                                        enable = false;
                                    }
                                    if (!roolhasPermission)
                                    {
                                        canUse = false;
                                    }
                                    permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
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
                //permissionReslut.AddRange(GetRolePermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions, userAllPremission));
                permissionReslut.AddRange(GerPermission(PermissionTypeEnum.UpdateRole, 0, heirIdList, menuList, permissionList, rolePermissions, userAllPremission));
                result.CurrencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.CurrencyList);
                result.Permission = permissionReslut;
            }
            return result;
        }

        public IEnumerable<AdminRole> GetAdminRole()
        {
            return permissionService.GetAdminRole();
        }
        public int AddAdminRole(int siteId, string roleName, string remark, IEnumerable<int> currencyList, List<SetPermission> permission, int userId)
        {
            return permissionService.AddAdminRole(siteId, roleName, remark, currencyList, permission, userId);
        }
        public int UpdateAdminRole(int role, string roleName, string remark, IEnumerable<int> currencyList, List<SetPermission> permission)
        {
            return permissionService.UpdateAdminRole(role, roleName, remark, currencyList, permission);
        }
        public void DeleteAdminRole(int role, IEnumerable<AdminUserV2> allUser)
        {
            List<UpdataAdminUserV2Role> updateRoleList = new List<UpdataAdminUserV2Role>();
            allUser.ToList().ForEach(x =>
            {
                var roleList = JsonConvert.DeserializeObject<List<int>>(x.Role);
                if (roleList.Exists(y => y == role))
                {
                    //updateRoleList.Add(x.UserId);
                    updateRoleList.Add(new UpdataAdminUserV2Role()
                    {
                        UserId = x.UserId,
                        Role = roleList.Where(z => z != role).ToList()
                    });
                }
            });
            permissionService.DeleteAdminRole(role, updateRoleList);
        }
        public IEnumerable<RoleList> GetRoleList(string roleName, int siteId)
        {
            var result = permissionService.GetRoleList(roleName, siteId).ToList();
            result.ForEach(x =>
            {
                if (x.CurrencyList != null)
                {
                    x.icurrencyList = JsonConvert.DeserializeObject<List<int>>(x.CurrencyList);
                }
                if (x.UserList != null)
                {
                    x.iuserList = JsonConvert.DeserializeObject<List<string>>(x.UserList);
                }
            });
            return result;
        }









        #endregion

        #region UserPermission
        public IEnumerable<UserPermissionData> GetUserPermissionData(int userId,int siteId)
        {
            return permissionService.GetUserPermissionData(userId, siteId);
        }
        public IEnumerable<UserList> GetAdminUserList(string userName,int siteId)
        {
            var result = permissionService.GetAdminUserList(userName, siteId).ToList();
            result.ForEach(x =>
            {
                if (x.CurrencyList != null)
                {
                    x.icurrencyList = JsonConvert.DeserializeObject<List<int>>(x.CurrencyList);
                }
            });
            return result;
        }
        public void DeleteAdminUser(int userId)
        {
            permissionService.DeleteAdminUser(userId);
        }
        public IEnumerable<int> GetAddAdminUserCurrency(AdminUserV2 loginUser)
        {
            if (loginUser.IsSuper)
            {
                var userSiteList = serverInfoService.GetSiteList().ToList();
                return userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;
            }
            else
            {
                return JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.CurrencyList);
            }
        }
        public AddUserPremission GetAddAdminUser(int role, AdminUserV2 loginUser)
        {
            AddUserPremission result = new AddUserPremission();
            List<Permission> permissionReslut = new List<Permission>();
            var menuList = GetAdminMenuDtoV2().ToList();
            var permissionList = GetAdminPermissionDtoV2().ToList();
            var heirIdList = menuList.Where(x => x.HeirId != 0).Select(x => x.HeirId).Distinct();
            var assignFuncDatas = menuList.Where(x => x.HeirId == 0 && !heirIdList.Contains(x.Id));
            var rolePermissions = permissionService.GetRolePermission(role).ToList();
            if (loginUser.IsSuper)
            {
                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    var rolePermission = rolePermissions.Find(x => x.MenuId == funcData.Id);
                    if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                    {
                        var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                        foreach (var permission in assignFuncAdditionalSetting.Permission)
                        {
                            var canUse = false;
                            var enable = true;
                            bool roolhasPermission = false;
                            if (rolePermission != null)
                            {
                                roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                            }
                            if (roolhasPermission)
                            {
                                canUse = true;
                                enable = false;
                            }
                            var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                            permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse ,Enable = enable });
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
                permissionReslut.AddRange(GetAddUserPermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions));
                result.Permission = permissionReslut;
            }
            else
            {
                var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                var userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();

                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    var userPermission = userAllPremission.Find(x => x.MenuId == funcData.Id);
                    var rolePermission = rolePermissions.Find(x => x.MenuId == funcData.Id);
                    if (userPermission != null || rolePermission != null)
                    {
                        if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                        {
                            var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                            foreach (var permission in assignFuncAdditionalSetting.Permission)
                            {
                                bool userhasPermission = false;
                                bool roolhasPermission = false;
                                if (userPermission != null)
                                {
                                    userhasPermission = userPermission.Permission.Exists(x => x == permission);
                                }
                                if (rolePermission != null)
                                {
                                    roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                }
                                if (userhasPermission || roolhasPermission)
                                {
                                    var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                    var enable = true;
                                    var canUse = true;
                                    if (roolhasPermission)
                                    {
                                        enable = false;
                                    }
                                    if (!roolhasPermission)
                                    {
                                        canUse = false;
                                    }
                                    permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
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
                permissionReslut.AddRange(GetAddUserPermissionMenu(0 ,heirIdList, menuList, permissionList, rolePermissions, userAllPremission));
                result.Permission = permissionReslut;
            }

            return result;
        }

        public int AddAdminUser(int siteId,int role, string userName, string password, IEnumerable<int> currencyList, List<SetPermission> permission)
        {
            var userPermission = GetUserPermission(role, permission);
            return permissionService.AddAdminUser(siteId, role, userName, password, currencyList, userPermission);
        }

        public UpdateUserPermission GetUpdateAdminUser(int role, int userId,int siteId, AdminUserV2 loginUser)
        {
            UpdateUserPermission result = new UpdateUserPermission();
            List<Permission> permissionReslut = new List<Permission>();
            var menuList = GetAdminMenuDtoV2().ToList();
            var permissionList = GetAdminPermissionDtoV2().ToList();
            var heirIdList = menuList.Where(x => x.HeirId != 0).Select(x => x.HeirId).Distinct();
            var assignFuncDatas = menuList.Where(x => x.HeirId == 0 && !heirIdList.Contains(x.Id));
            var rolePermissions = permissionService.GetRolePermission(role).ToList();
            var Permission = permissionService.GetUserPermissionData(userId, siteId).ToList();
            if (loginUser.IsSuper)
            {
                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    var rolePermission = rolePermissions.Find(x => x.MenuId == funcData.Id);
                    var updateUserPermission = Permission.Find(x => x.MenuId == funcData.Id);
                    if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                    {
                        var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                        foreach (var permission in assignFuncAdditionalSetting.Permission)
                        {
                            var enable = true;
                            var canUse = false;
                            bool rolehasPermission = false;
                            bool updateUserhasPermission = false;
                            if (rolePermission != null)
                            {
                                rolehasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                            }
                            if (updateUserPermission != null)
                            {
                                updateUserhasPermission = JsonConvert.DeserializeObject<List<int>>(updateUserPermission.Permission).Exists(x => x == permission);
                            }
                            if (rolehasPermission || updateUserhasPermission)
                            {
                                canUse = true;
                            }
                            if (rolehasPermission)
                            {
                                enable = false;
                            }
                            var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                            permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
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
                permissionReslut.AddRange(GetUpdateUserPermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions, Permission));
                var userSiteList = serverInfoService.GetSiteList().ToList();
                result.CurrencyList = userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;
                result.Permission = permissionReslut;
            }
            else
            {
                var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                var userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();

                foreach (var funcData in assignFuncDatas)
                {
                    var permissionData = new List<PermissionSubFuncKeys>();
                    var userPermission = userAllPremission.Find(x => x.MenuId == funcData.Id);
                    var rolePermission = rolePermissions.Find(x => x.MenuId == funcData.Id);
                    var updateUserPermission = Permission.Find(x => x.MenuId == funcData.Id);
                    if (userPermission != null || rolePermission != null || updateUserPermission != null)
                    {
                        if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                        {
                            var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                            foreach (var permission in assignFuncAdditionalSetting.Permission)
                            {
                                bool userhasPermission = false;
                                bool rolehasPermission = false;
                                bool updateUserhasPermission = false;
                                if (userPermission != null)
                                {
                                    userhasPermission = userPermission.Permission.Exists(x => x == permission);
                                }
                                if (rolePermission != null)
                                {
                                    rolehasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                }
                                if(updateUserPermission != null)
                                {
                                    updateUserhasPermission = JsonConvert.DeserializeObject<List<int>>(updateUserPermission.Permission).Exists(x => x == permission);
                                }
                                if (userhasPermission || rolehasPermission || updateUserhasPermission)
                                {
                                    var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                    var enable = true;
                                    var canUse = false;
                                    if(rolehasPermission || updateUserhasPermission)
                                    {
                                        canUse = true;
                                    }
                                    if (rolehasPermission)
                                    {
                                        enable = false;
                                    }
                                    if(updateUserhasPermission && !userhasPermission)
                                    {
                                        enable = false;
                                    }
                                    permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
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
                permissionReslut.AddRange(GetUpdateUserPermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions, Permission,userAllPremission));
                result.CurrencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.CurrencyList);
                result.Permission = permissionReslut;
            }

            return result;
        }

        public int UpdateAdminUser(int siteId, int userId, int role, string userName, string password, IEnumerable<int> currencyList, List<SetPermission> permission)
        {
            var userPermission = GetUserPermission(role, permission);
            return permissionService.UpdateAdminUser(siteId, userId, role, userName, password, currencyList, userPermission);
        }
        public int SetAdminUserPassword(int userId, string password)
        {
            return permissionService.SetAdminUserPassword(userId, password);
        }
        public int SetAdminUserStatus(int userId, int status)
        {
            return permissionService.SetAdminUserStatus(userId, status);
        }
        #endregion

        #region Common
        public IEnumerable<AdminMenuDtoV2> GetAdminMenuDtoV2()
        {
            return permissionService.GetAdminMenuDtoV2();
        }
        public IEnumerable<AdminPermissionDtoV2> GetAdminPermissionDtoV2()
        {
            return permissionService.GetAdminPermissionDtoV2();
        }
        #endregion

        #region Private
        //private List<Permission> GetAllRolePermissionMenu(int moduleIId,
        //   IEnumerable<int> heirIdList,
        //   IEnumerable<AdminMenuDtoV2> menuList,
        //   IEnumerable<AdminPermissionDtoV2> permissionList,
        //   IEnumerable<UserAllPremission> userPermissionList = null)
        //{
        //    var result = new List<Permission>();
        //    var assignModuleList = menuList.Where(x => x.HeirId == moduleIId);

        //    foreach (var moduleData in assignModuleList)
        //    {
        //        var FunctionDatas = new List<FunctionDatas>();
        //        var SubLayerDatas = new List<Permission>();

        //        var assignFuncDatas = menuList.Where(x => x.HeirId == moduleData.Id);

        //        foreach (var funcData in assignFuncDatas)
        //        {
        //            if (userPermissionList != null)
        //            {
        //                var userPermission = userPermissionList.ToList().Find(x => x.MenuId == funcData.Id);

        //                if (userPermission != null)
        //                {
        //                    var permissionData = new List<PermissionSubFuncKeys>();
        //                    if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
        //                    {
        //                        var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
        //                        foreach (var permission in assignFuncAdditionalSetting.Permission)
        //                        {
        //                            if (userPermission.Permission.Exists(x => x == permission))
        //                            {
        //                                var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
        //                                permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = false });
        //                            }
        //                        }
        //                        FunctionDatas.Add(new FunctionDatas()
        //                        {
        //                            DisPlayKey = funcData.Id,
        //                            Name = funcData.Name,
        //                            Settings = permissionData
        //                        });
        //                    }
        //                    else
        //                        continue;
        //                }
        //            }
        //            else
        //            {
        //                var permissionData = new List<PermissionSubFuncKeys>();
        //                if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
        //                {
        //                    var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
        //                    foreach (var permission in assignFuncAdditionalSetting.Permission)
        //                    {
        //                        var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
        //                        permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = false });
        //                    }
        //                    FunctionDatas.Add(new FunctionDatas()
        //                    {
        //                        DisPlayKey = funcData.Id,
        //                        Name = funcData.Name,
        //                        Settings = permissionData
        //                    });
        //                }
        //                else
        //                    continue;
        //            }
        //        }

        //        if (heirIdList.Contains(moduleData.Id))
        //        {
        //            var subLayerDatas = GetAllRolePermissionMenu(moduleData.Id, heirIdList, menuList, permissionList, userPermissionList);
        //            SubLayerDatas.AddRange(subLayerDatas);
        //        }

        //        if (FunctionDatas.Any() || SubLayerDatas.Any())
        //        {
        //            result.Add(new Permission()
        //            {
        //                ModuleId = moduleData.Id,
        //                Name = moduleData.Name,
        //                FunctionDatas = FunctionDatas,
        //                SubLayerDatas = SubLayerDatas
        //            });
        //        }
        //    }
        //    return result;
        //}
        //private List<Permission> GetRolePermissionMenu(int moduleIId,
        //   IEnumerable<int> heirIdList,
        //   IEnumerable<AdminMenuDtoV2> menuList,
        //   IEnumerable<AdminPermissionDtoV2> permissionList,
        //   IEnumerable<RolePermission> rolePermissionsList,
        //   IEnumerable<UserAllPremission> userPermissionList = null
        //   )
        //{
        //    var result = new List<Permission>();
        //    var assignModuleList = menuList.Where(x => x.HeirId == moduleIId);

        //    foreach (var moduleData in assignModuleList)
        //    {
        //        var FunctionDatas = new List<FunctionDatas>();
        //        var SubLayerDatas = new List<Permission>();

        //        var assignFuncDatas = menuList.Where(x => x.HeirId == moduleData.Id);

        //        foreach (var funcData in assignFuncDatas)
        //        {

        //            if (userPermissionList != null)
        //            {
        //                var userPermission = userPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
        //                var rolePermission = rolePermissionsList.ToList().Find(x => x.MenuId == funcData.Id);
        //                if (userPermission != null || rolePermission != null)
        //                {
        //                    var permissionData = new List<PermissionSubFuncKeys>();
        //                    if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
        //                    {
        //                        var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
        //                        foreach (var permission in assignFuncAdditionalSetting.Permission)
        //                        {
        //                            bool userhasPermission = false;
        //                            bool roolhasPermission = false;
        //                            if (userPermission != null)
        //                            {
        //                                userhasPermission = userPermission.Permission.Exists(x => x == permission);
        //                            }
        //                            if (rolePermission != null)
        //                            {
        //                                roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
        //                            }
        //                            if (userhasPermission || roolhasPermission)
        //                            {
        //                                var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
        //                                var enable = true;
        //                                var canUse = true;
        //                                if (!userhasPermission)
        //                                {
        //                                    enable = false;
        //                                }
        //                                if (!roolhasPermission)
        //                                {
        //                                    canUse = false;
        //                                }
        //                                permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
        //                            }
        //                        }
        //                        FunctionDatas.Add(new FunctionDatas()
        //                        {
        //                            DisPlayKey = funcData.Id,
        //                            Name = funcData.Name,
        //                            Settings = permissionData
        //                        });
        //                    }
        //                    else
        //                        continue;
        //                }
        //            }
        //            else
        //            {
        //                var permissionData = new List<PermissionSubFuncKeys>();
        //                var rolePermission = rolePermissionsList.ToList().Find(x => x.MenuId == funcData.Id);
        //                if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
        //                {
        //                    var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
        //                    foreach (var permission in assignFuncAdditionalSetting.Permission)
        //                    {
        //                        var canUse = true;
        //                        if (rolePermission == null)
        //                        {
        //                            canUse = false;
        //                        }
        //                        else
        //                        {
        //                            bool roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
        //                            if (!roolhasPermission)
        //                            {
        //                                canUse = false;
        //                            }
        //                        }
        //                        var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
        //                        permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse });
        //                    }
        //                    FunctionDatas.Add(new FunctionDatas()
        //                    {
        //                        DisPlayKey = funcData.Id,
        //                        Name = funcData.Name,
        //                        Settings = permissionData
        //                    });
        //                }
        //                else
        //                    continue;
        //            }
        //        }

        //        if (heirIdList.Contains(moduleData.Id))
        //        {
        //            var subLayerDatas = GetRolePermissionMenu(moduleData.Id, heirIdList, menuList, permissionList, rolePermissionsList, userPermissionList);
        //            SubLayerDatas.AddRange(subLayerDatas);
        //        }

        //        if (FunctionDatas.Any() || SubLayerDatas.Any())
        //        {
        //            result.Add(new Permission()
        //            {
        //                ModuleId = moduleData.Id,
        //                Name = moduleData.Name,
        //                FunctionDatas = FunctionDatas,
        //                SubLayerDatas = SubLayerDatas
        //            });
        //        }
        //    }
        //    return result;
        //}

        private List<Permission> GerPermission(
            PermissionTypeEnum permissionType,
            int moduleIId,
           IEnumerable<int> heirIdList,
           IEnumerable<AdminMenuDtoV2> menuList,
           IEnumerable<AdminPermissionDtoV2> permissionList,
           IEnumerable<RolePermission> rolePermissionsList = null,
           IEnumerable<UserAllPremission> userPermissionList = null
            )
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
                    var permissionData = new List<PermissionSubFuncKeys>();
                    if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                    {
                        if (userPermissionList != null)
                        {
                            UserAllPremission userPermission = new UserAllPremission();
                            RolePermission rolePermission = new RolePermission();
                            userPermission = userPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
                            if (permissionType == PermissionTypeEnum.AddRole)
                            {
                                if (userPermission != null)
                                {
                                    var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                                    foreach (var permission in assignFuncAdditionalSetting.Permission)
                                    {
                                        if (userPermission.Permission.Exists(x => x == permission))
                                        {
                                            var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                            permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = false });
                                        }
                                    }
                                    FunctionDatas.Add(new FunctionDatas()
                                    {
                                        DisPlayKey = funcData.Id,
                                        Name = funcData.Name,
                                        Settings = permissionData
                                    });
                                }
                            }
                            if (permissionType == PermissionTypeEnum.UpdateRole)
                            {
                                if (rolePermissionsList != null)
                                {
                                    rolePermission = rolePermissionsList.ToList().Find(x => x.MenuId == funcData.Id);
                                }
                                if (userPermission != null || rolePermission != null)
                                {
                                    var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                                    foreach (var permission in assignFuncAdditionalSetting.Permission)
                                    {
                                        bool userhasPermission = false;
                                        bool roolhasPermission = false;
                                        if (userPermission != null)
                                        {
                                            userhasPermission = userPermission.Permission.Exists(x => x == permission);
                                        }
                                        if (rolePermission != null)
                                        {
                                            roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                        }
                                        if (userhasPermission || roolhasPermission)
                                        {
                                            var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                            var enable = true;
                                            var canUse = true;
                                            if (!userhasPermission)
                                            {
                                                enable = false;
                                            }
                                            if (!roolhasPermission)
                                            {
                                                canUse = false;
                                            }
                                            permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
                                        }
                                    }
                                    FunctionDatas.Add(new FunctionDatas()
                                    {
                                        DisPlayKey = funcData.Id,
                                        Name = funcData.Name,
                                        Settings = permissionData
                                    });
                                }
                            }
                        }
                        else
                        {
                            RolePermission rolePermission = new RolePermission();
                            if (permissionType == PermissionTypeEnum.AddRole)
                            {
                                var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                                foreach (var permission in assignFuncAdditionalSetting.Permission)
                                {
                                    var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                    permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = false });
                                }
                                FunctionDatas.Add(new FunctionDatas()
                                {
                                    DisPlayKey = funcData.Id,
                                    Name = funcData.Name,
                                    Settings = permissionData
                                });
                            }
                            if (permissionType == PermissionTypeEnum.UpdateRole)
                            {
                                if (rolePermissionsList != null)
                                {
                                    rolePermission = rolePermissionsList.ToList().Find(x => x.MenuId == funcData.Id);
                                    var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                                    foreach (var permission in assignFuncAdditionalSetting.Permission)
                                    {
                                        var canUse = true;
                                        if (rolePermission == null)
                                        {
                                            canUse = false;
                                        }
                                        else
                                        {
                                            bool roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                            if (!roolhasPermission)
                                            {
                                                canUse = false;
                                            }
                                        }
                                        var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                        permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse });
                                    }
                                    FunctionDatas.Add(new FunctionDatas()
                                    {
                                        DisPlayKey = funcData.Id,
                                        Name = funcData.Name,
                                        Settings = permissionData
                                    });
                                }
                            }
                        }
                    }
                    else
                        continue;
                }

                if (heirIdList.Contains(moduleData.Id))
                {
                    var subLayerDatas = GerPermission(permissionType, moduleData.Id, heirIdList, menuList, permissionList, rolePermissionsList, userPermissionList);
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

        private List<Permission> GetAddUserPermissionMenu(int moduleIId,
           IEnumerable<int> heirIdList,
           IEnumerable<AdminMenuDtoV2> menuList,
           IEnumerable<AdminPermissionDtoV2> permissionList,
           IEnumerable<RolePermission> rolePermissionsList,
           IEnumerable<UserAllPremission> userPermissionList = null
           )
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

                    if (userPermissionList != null)
                    {
                        var userPermission = userPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
                        var rolePermission = rolePermissionsList.ToList().Find(x => x.MenuId == funcData.Id);
                        if (userPermission != null || rolePermission != null)
                        {
                            var permissionData = new List<PermissionSubFuncKeys>();
                            if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                            {
                                var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                                foreach (var permission in assignFuncAdditionalSetting.Permission)
                                {
                                    bool userhasPermission = false;
                                    bool roolhasPermission = false;
                                    if (userPermission != null)
                                    {
                                        userhasPermission = userPermission.Permission.Exists(x => x == permission);
                                    }
                                    if (rolePermission != null)
                                    {
                                        roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                    }
                                    if (userhasPermission || roolhasPermission)
                                    {
                                        var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                        var enable = true;
                                        var canUse = true;

                                        if (roolhasPermission)
                                        {
                                            enable = false;
                                        }
                                        else
                                        {
                                            canUse = false;
                                        }

                                        permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
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
                        var rolePermission = rolePermissionsList.ToList().Find(x => x.MenuId == funcData.Id);
                        if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                        {
                            var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                            foreach (var permission in assignFuncAdditionalSetting.Permission)
                            {
                                var canUse = true;
                                var enable = true;
                                if (rolePermission == null)
                                {
                                    canUse = false;
                                }
                                else
                                {
                                    bool roolhasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                    if (!roolhasPermission)
                                    {
                                        canUse = false;
                                    }
                                    else
                                    {
                                        enable = false;
                                    }
                                }
                                var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
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
                    var subLayerDatas = GetAddUserPermissionMenu(moduleData.Id, heirIdList, menuList, permissionList, rolePermissionsList, userPermissionList);
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

        private List<Permission> GetUpdateUserPermissionMenu(int moduleIId,
           IEnumerable<int> heirIdList,
           IEnumerable<AdminMenuDtoV2> menuList,
           IEnumerable<AdminPermissionDtoV2> permissionList,
           IEnumerable<RolePermission> rolePermissionsList,
           IEnumerable<UserPermissionData> updateUserPermissionList,
           IEnumerable<UserAllPremission> userPermissionList = null
           )
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

                    if (userPermissionList != null)
                    {
                        var userPermission = userPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
                        var rolePermission = rolePermissionsList.ToList().Find(x => x.MenuId == funcData.Id);
                        var updateUserPermission = updateUserPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
                        if (userPermission != null || rolePermission != null)
                        {
                            var permissionData = new List<PermissionSubFuncKeys>();
                            if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                            {
                                var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                                foreach (var permission in assignFuncAdditionalSetting.Permission)
                                {
                                    bool userhasPermission = false;
                                    bool rolehasPermission = false;
                                    bool updateUserhasPermission = false;
                                    if (userPermission != null)
                                    {
                                        userhasPermission = userPermission.Permission.Exists(x => x == permission);
                                    }
                                    if (rolePermission != null)
                                    {
                                        rolehasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                    }
                                    if (updateUserPermission != null)
                                    {
                                        updateUserhasPermission = JsonConvert.DeserializeObject<List<int>>(updateUserPermission.Permission).Exists(x => x == permission);
                                    }
                                    if (userhasPermission || rolehasPermission || updateUserhasPermission)
                                    {
                                        var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                        var enable = true;
                                        var canUse = false;
                                        if (rolehasPermission || updateUserhasPermission)
                                        {
                                            canUse = true;
                                        }
                                        if (rolehasPermission)
                                        {
                                            enable = false;
                                        }
                                        if (updateUserhasPermission && !userhasPermission)
                                        {
                                            enable = false;
                                        }

                                        permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
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
                        var rolePermission = rolePermissionsList.ToList().Find(x => x.MenuId == funcData.Id);
                        var updateUserPermission = updateUserPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
                        if (!string.IsNullOrWhiteSpace(funcData.AdditionalSetting))
                        {
                            var assignFuncAdditionalSetting = JsonConvert.DeserializeObject<AdditionalSetting>(funcData.AdditionalSetting);
                            foreach (var permission in assignFuncAdditionalSetting.Permission)
                            {
                                var enable = true;
                                var canUse = false;
                                bool rolehasPermission = false;
                                bool updateUserhasPermission = false;
                                if (rolePermission != null)
                                {
                                    rolehasPermission = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(x => x == permission);
                                }
                                if (updateUserPermission != null)
                                {
                                    updateUserhasPermission = JsonConvert.DeserializeObject<List<int>>(updateUserPermission.Permission).Exists(x => x == permission);
                                }

                                if (rolehasPermission || updateUserhasPermission)
                                {
                                    canUse = true;
                                }
                                if (rolehasPermission)
                                {
                                    enable = false;
                                }
                                var permissionName = permissionList.Where(x => x.Id == permission).Select(x => x.Name).FirstOrDefault();
                                permissionData.Add(new PermissionSubFuncKeys() { PermissionId = permission, Name = permissionName, CanUse = canUse, Enable = enable });
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
                    var subLayerDatas = GetUpdateUserPermissionMenu(moduleData.Id, heirIdList, menuList, permissionList, rolePermissionsList, updateUserPermissionList, userPermissionList);
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

        private List<SetPermission> GetUserPermission(int role, List<SetPermission> permission)
        {
            List<SetPermission> userPermission = new List<SetPermission>();
            var rolePermissions = permissionService.GetRolePermission(role).ToList();
            permission.ForEach(x =>
            {
                if (rolePermissions.Exists(y => y.MenuId == x.DisPlayKey))
                {
                    var rolePermission = rolePermissions.Find(t => t.MenuId == x.DisPlayKey);
                    var permissionIdList = JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission);
                    List<int> tempPermissionIdList = new List<int>();
                    x.PermissionIdList.ForEach(z =>
                    {
                        if (!JsonConvert.DeserializeObject<List<int>>(rolePermission.Permission).Exists(s => s == z))
                        {
                            tempPermissionIdList.Add(z);
                        }
                    });
                    if (tempPermissionIdList.Count > 0)
                    {
                        userPermission.Add(new SetPermission() { DisPlayKey = x.DisPlayKey, PermissionIdList = tempPermissionIdList });
                    }
                }
                else
                {
                    userPermission.Add(x);
                }
            });
            return userPermission;
        }
        #endregion

        #endregion   
    }
}
