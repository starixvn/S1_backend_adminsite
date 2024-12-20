using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class PermissionBLL
    {
        private readonly PermissionService _permissionService;
        private readonly ServerInfoService _serverInfoService;
        private readonly AdminService _adminService;
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly NewAdminUserService _newAdminUserService;

        public PermissionBLL(IServiceProvider provider)
        {
            _permissionService = provider.GetRequiredService<PermissionService>();
            _serverInfoService = provider.GetRequiredService<ServerInfoService>();
            _adminService = provider.GetRequiredService<AdminService>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _newAdminUserService = provider.GetRequiredService<NewAdminUserService>();
        }

        public bool CheckPermission(AdminUserV2 loginUser, int menuId, int permissionId)
        {
            if (loginUser.IsSuper) return true;

            if (loginUser.Status == 0) return false;

            var userAllPermission = new List<UserAllPremission>();
            if (string.IsNullOrEmpty(loginUser.Role))
            {
                userAllPermission = GetUserAllPremission(null, loginUser.UserId, loginUser.SiteId).ToList();
            }
            else
            {
                var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                userAllPermission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
            }

            var permissionList = userAllPermission.Find(x => x.MenuId == menuId);
            return permissionList != null && permissionList.Permission.Exists(x => x == permissionId);
        }

        #region New Admin-Administration

        #region RolePermission
        public IEnumerable<RolePermission> GetRolePermission(int role)
        {
            return _permissionService.GetRolePermission(role);
        }
        public IEnumerable<UserAllPremission> GetUserAllPremission(int? role, int userId, int siteId)
        {
            List<UserAllPremission> userAllPremission = new List<UserAllPremission>();
            if (role != null)
            {
                var rolePermission = _permissionService.GetRolePermission(role.Value).ToList();
                rolePermission.ForEach(x =>
                {
                    var permission = JsonConvert.DeserializeObject<List<int>>(x.Permission);
                    userAllPremission.Add(new UserAllPremission() { MenuId = x.MenuId, Permission = permission });
                });
            }

            var Permission = _permissionService.GetUserPermissionData(userId, siteId).ToList();
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
            var menuList = _adminService.GetAllAdminMenu().Where(x => x.Status == AdminFunctionStatusEnum.Enable);
            var permissionList = _adminService.GetAllAdminPermission().Where(x => x.Status == AdminPermissionStatusEnum.Enable); ;
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
                var userSiteList = _serverInfoService.GetSiteList().ToList();
                result.CurrencyList = userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;
                result.Permission = permissionReslut;
            }
            else
            {
                //var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                //var userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
                var userAllPremission = new List<UserAllPremission>();
                if (loginUser.Role == null)
                {
                    userAllPremission = GetUserAllPremission(null, loginUser.UserId, loginUser.SiteId).ToList();
                }
                else
                {
                    var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                    userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
                }
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
            var menuList = _adminService.GetAllAdminMenu().Where(x => x.Status == AdminFunctionStatusEnum.Enable);
            var permissionList = _adminService.GetAllAdminPermission().Where(x => x.Status == AdminPermissionStatusEnum.Enable); ;
            var heirIdList = menuList.Where(x => x.HeirId != 0).Select(x => x.HeirId).Distinct();
            var assignFuncDatas = menuList.Where(x => x.HeirId == 0 && !heirIdList.Contains(x.Id));
            var rolePermissions = _permissionService.GetRolePermission(role).ToList();
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
                var userSiteList = _serverInfoService.GetSiteList().ToList();
                result.CurrencyList = userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;
                result.Permission = permissionReslut;
            }
            else
            {
                //var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                //var userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
                var userAllPremission = new List<UserAllPremission>();
                if (loginUser.Role == null)
                {
                    userAllPremission = GetUserAllPremission(null, loginUser.UserId, loginUser.SiteId).ToList();
                }
                else
                {
                    var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                    userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
                }

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
                if (loginUser.CurrencyList != null)
                {
                    result.CurrencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.CurrencyList);
                }
                else
                {
                    result.CurrencyList = new List<int>();
                }

                result.Permission = permissionReslut;
            }
            return result;
        }

        public IEnumerable<AdminRole> GetAdminRole()
        {
            return _permissionService.GetAdminRole();
        }
        public int AddAdminRole(int siteId, string roleName, string remark, IEnumerable<int> currencyList, List<SetPermission> permission, int userId, string Operator)
        {
            return _permissionService.AddAdminRole(siteId, roleName, remark, currencyList, permission, userId, Operator);
        }
        public int UpdateAdminRole(int role, string roleName, string remark, IEnumerable<int> currencyList, List<SetPermission> permission, string Operator)
        {
            return _permissionService.UpdateAdminRole(role, roleName, remark, currencyList, permission, Operator);
        }
        public void DeleteAdminRole(int role)
        {
            _permissionService.DeleteAdminRole(role);
        }
        public IEnumerable<RoleList> GetRoleList(string roleName, int siteId)
        {
            var result = _permissionService.GetRoleList(roleName, siteId).ToList();
            result.ForEach(x =>
            {
                if (x.CurrencyList != null)
                {
                    x.icurrencyList = JsonConvert.DeserializeObject<List<int>>(x.CurrencyList);
                }
                if (x.UserList != null)
                {
                    x.iuserList = JsonConvert.DeserializeObject<List<UserStatus>>(x.UserList);
                }
            });
            return result;
        }

        #endregion

        #region UserPermission
        public IEnumerable<UserPermissionData> GetUserPermissionData(int userId, int siteId)
        {
            return _permissionService.GetUserPermissionData(userId, siteId);
        }
        public IEnumerable<UserList> GetAdminUserList(string userName, int siteId, bool status)
        {
            var result = _permissionService.GetAdminUserList(userName, siteId, status).ToList();
            result.ForEach(x =>
            {
                if (x.CurrencyList != null)
                {
                    x.icurrencyList = JsonConvert.DeserializeObject<List<int>>(x.CurrencyList);
                }
            });
            return result;
        }
        //public void DeleteAdminUser(int userId)
        //{
        //    permissionService.DeleteAdminUser(userId);
        //}
        public IEnumerable<int> GetAddAdminUserCurrency(AdminUserV2 loginUser)
        {
            if (loginUser.IsSuper)
            {
                var userSiteList = _serverInfoService.GetSiteList().ToList();
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
            var menuList = _adminService.GetAllAdminMenu().Where(x => x.Status == AdminFunctionStatusEnum.Enable);
            var permissionList = _adminService.GetAllAdminPermission().Where(x => x.Status == AdminPermissionStatusEnum.Enable); ;
            var heirIdList = menuList.Where(x => x.HeirId != 0).Select(x => x.HeirId).Distinct();
            var assignFuncDatas = menuList.Where(x => x.HeirId == 0 && !heirIdList.Contains(x.Id));
            var rolePermissions = _permissionService.GetRolePermission(role).ToList();
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
                permissionReslut.AddRange(GetUserPermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions));
                result.Permission = permissionReslut;
            }
            else
            {
                var userAllPremission = new List<UserAllPremission>();
                if (loginUser.Role == null)
                {
                    userAllPremission = GetUserAllPremission(null, loginUser.UserId, loginUser.SiteId).ToList();
                }
                else
                {
                    var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                    userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
                }

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
                permissionReslut.AddRange(GetUserPermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions, userAllPremission));
                result.Permission = permissionReslut;
            }

            return result;
        }

        public int AddAdminUser(int siteId, int role, string userName, string password, IEnumerable<int> currencyList, List<SetPermission> permission, string Operator)
        {
            var userPermission = GetUserPermission(role, permission);
            return _permissionService.AddAdminUser(siteId, role, userName, password, currencyList, userPermission, Operator);
        }

        public UpdateUserPermission GetUpdateAdminUser(int role, int userId, int siteId, AdminUserV2 loginUser)
        {
            UpdateUserPermission result = new UpdateUserPermission();
            List<Permission> permissionReslut = new List<Permission>();
            var menuList = _adminService.GetAllAdminMenu().Where(x => x.Status == AdminFunctionStatusEnum.Enable);
            var permissionList = _adminService.GetAllAdminPermission().Where(x => x.Status == AdminPermissionStatusEnum.Enable);
            var heirIdList = menuList.Where(x => x.HeirId != 0).Select(x => x.HeirId).Distinct();
            var assignFuncDatas = menuList.Where(x => x.HeirId == 0 && !heirIdList.Contains(x.Id));
            var rolePermissions = _permissionService.GetRolePermission(role).ToList();
            var Permission = _permissionService.GetUserPermissionData(userId, siteId).ToList();
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
                permissionReslut.AddRange(GetUserPermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions, null, Permission));
                var userSiteList = _serverInfoService.GetSiteList().ToList();
                result.CurrencyList = userSiteList.FirstOrDefault(x => x.SiteId == loginUser.SiteId).ICurrencyList;
                result.Permission = permissionReslut;
            }
            else
            {
                var userAllPremission = new List<UserAllPremission>();
                if (loginUser.Role == null)
                {
                    userAllPremission = GetUserAllPremission(null, loginUser.UserId, loginUser.SiteId).ToList();
                }
                else
                {
                    var userRole = JsonConvert.DeserializeObject<List<int>>(loginUser.Role).FirstOrDefault();
                    userAllPremission = GetUserAllPremission(userRole, loginUser.UserId, loginUser.SiteId).ToList();
                }

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
                permissionReslut.AddRange(GetUserPermissionMenu(0, heirIdList, menuList, permissionList, rolePermissions, userAllPremission, Permission));
                if (loginUser.CurrencyList != null)
                {
                    result.CurrencyList = JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.CurrencyList);
                }
                else
                {
                    result.CurrencyList = new List<int>();
                }

                result.Permission = permissionReslut;
            }

            return result;
        }

        public int UpdateAdminUser(int siteId, int userId, int role, IEnumerable<int> currencyList, List<SetPermission> permission, string Operator)
        {
            var userPermission = GetUserPermission(role, permission);
            return _permissionService.UpdateAdminUser(siteId, userId, role, currencyList, userPermission, Operator);
        }
        public bool SetAdminUserPassword(ParamsWithLog<SetAdminUserPasswordReq> input)
        {
            return _permissionService.SetAdminUserPassword(input);
        }
        public bool SetAdminUserStatus(ParamsWithLog<SetAdminUserStatusReq> input)
        {
            return _permissionService.SetAdminUserStatus(input);
        }
        #endregion

        #region AssignBrand
        public IEnumerable<AdminBrandRole> GetAdminBrandRole(int role)
        {
            return _permissionService.GetAdminBrandRole(role);
        }
        public IEnumerable<AdminBrandData> GetAssignBrand(IEnumerable<int> selectSite, string userName)
        {
            var assignBrand = _permissionService.GetAssignBrand(selectSite, userName).ToList();

            var assignBrandData = assignBrand.GroupBy(x => (x.UserId, x.UserName), (key, value) =>
             new AdminBrandData
             {
                 UserId = key.UserId,
                 UserName = key.UserName,
                 Brand = value.Select(y => new UserBrand { SiteName = y.SiteName, SiteId = y.SiteId }),
                 UpdateTime = value.Max(y => y.UpdateTime),
                 LoginTime = value.Max(y => y.LoginTime)
             }).ToList();

            return assignBrandData;
        }
        public IEnumerable<UserBrand> GetUserBrand(int userId)
        {
            return _permissionService.GetUserBrand(userId);
        }
        public bool SetAdminBrand(SetAdminBrandReq input, string userName, List<UserBrand> beforeUserBrands, List<UserBrand> afterUserBrands)
        {
            return _permissionService.SetAdminBrand(input, userName, beforeUserBrands, afterUserBrands);
        }
        #endregion

        #region Private

        private List<Permission> GerPermission(
            PermissionTypeEnum permissionType,
            int moduleIId,
           IEnumerable<int> heirIdList,
           IEnumerable<AdminMenuDto> menuList,
           IEnumerable<AdminPermissionDto> permissionList,
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

        private List<Permission> GetUserPermissionMenu(int moduleIId,
           IEnumerable<int> heirIdList,
           IEnumerable<AdminMenuDto> menuList,
           IEnumerable<AdminPermissionDto> permissionList,
           IEnumerable<RolePermission> rolePermissionsList,
           IEnumerable<UserAllPremission> userPermissionList = null,
           IEnumerable<UserPermissionData> updateUserPermissionList = null
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
                        //var updateUserPermission = updateUserPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
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
                                    if (updateUserPermissionList != null)
                                    {
                                        var updateUserPermission = updateUserPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
                                        if (updateUserPermission != null)
                                        {
                                            updateUserhasPermission = JsonConvert.DeserializeObject<List<int>>(updateUserPermission.Permission).Exists(x => x == permission);
                                        }
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
                        //var updateUserPermission = updateUserPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
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
                                if (updateUserPermissionList != null)
                                {
                                    var updateUserPermission = updateUserPermissionList.ToList().Find(x => x.MenuId == funcData.Id);
                                    if (updateUserPermission != null)
                                    {
                                        updateUserhasPermission = JsonConvert.DeserializeObject<List<int>>(updateUserPermission.Permission).Exists(x => x == permission);
                                    }
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
                    var subLayerDatas = GetUserPermissionMenu(moduleData.Id, heirIdList, menuList, permissionList, rolePermissionsList, userPermissionList, updateUserPermissionList);
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
            var rolePermissions = _permissionService.GetRolePermission(role).ToList();
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

        #region Check Permission New

        public IEnumerable<UserAllPremission> GetUserAllPermission()
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var siteId = _newAuthorizeBll.CheckBrand();
            var loginUser = _newAdminUserService.GetUserV2(authData.UserId, siteId);

            if (loginUser.Role == null)
            {
                return GetUserAllPremission(null, authData.UserId, siteId);
            }

            var userRole = JsonConvert.DeserializeObject<IEnumerable<int>>(loginUser.Role);

            return GetUserAllPremission(userRole?.FirstOrDefault(), authData.UserId, siteId);
        }

        public IEnumerable<int> GetUserPermissionByMenuId(int menuId)
        {
            var permissions = GetUserAllPermission();

            return permissions.FirstOrDefault(x => x.MenuId == menuId)?.Permission;
        }

        public bool CheckPermission(int menuId, IEnumerable<int> permissions, bool matchAll = true)
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var siteId = _newAuthorizeBll.CheckBrand();
            var loginUser = _newAdminUserService.GetUserV2(authData.UserId, siteId);

            if (loginUser.IsSuper)
                return true;

            var userPermission = GetUserPermissionByMenuId(menuId);

            return matchAll
                ? permissions.All(permission => userPermission?.Contains(permission) ?? false)
                : permissions.Any(permission => userPermission?.Contains(permission) ?? false);
        }

        #endregion

        #endregion
    }
}
