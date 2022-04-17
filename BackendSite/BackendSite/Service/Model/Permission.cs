using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    [Serializable]
    public class FuncPermissionModel
    {
        public string DisplayKey { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FuncKey { get; set; }
        public List<FuncPermissionSettings> Settings { get; set; }
    }

    [Serializable]
    public class FuncPermissionSettings
    {
        public string DisplayKey { get; set; }
        public string FuncKey { get; set; }
        public bool Enable { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<SubFunctionKey> SubFuncKeys { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InputBoxType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<int> CheckedIndex { get; set; }

        public bool IsGroupSetting { get; set; }
        public IEnumerable<int> GroupCheckedIndex { get; set; }
        public IEnumerable<int> UserCheckedIndex { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<FuncPermissionSettings> FuncKeys { get; set; }
    }

    [Serializable]
    public class SubFunctionKey
    {
        public string DisplayKey { get; set; }
        public string FuncKey { get; set; }
    }

    public class GroupPermisison
    {
        public int GroupSEQ { get; set; }
        public string FuncKey { get; set; }
        public string SubFuncKeys { get; set; }
    }

    public class UserPermission
    {
        public string FuncKey { get; set; }
        public string SubFuncKeys { get; set; }
    }

    public class NewPermission
    {
        public IEnumerable<int> CurrencyList { get; set; }
        public List<Permission> Permission { get; set; }
    }
    public class Permission
    {
        public int? ModuleId { get; set; }
        public int DisPlayKey { get; set; }
        public String Name { get; set; }
        //public List<PermissionSettings> Settings { get; set; }
        public List<PermissionSubFuncKeys> Settings { get; set; }
        public List<FunctionDatas> FunctionDatas { get; set; }
        public List<Permission> SubLayerDatas { get; set; }
    }
    public class FunctionDatas
    {
        public int DisPlayKey { get; set; }
        public String Name { get; set; }
        public List<PermissionSubFuncKeys> Settings { get; set; }
    }
    public class PermissionSettings
    {
        public String DisPlayKey { get; set; }
        public int FunKey { get; set; }
        public List<PermissionSubFuncKeys> SubFuncKeys { get; set; }

    }
    public class PermissionSubFuncKeys
    {
        public int PermissionId { get; set; }
        public String Name { get; set; }
        public bool CanUse { get; set; }
        public bool Enable { get; set; } = true;
    }

    public class AdminMenuDtoV2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int HeirId { get; set; }
        public int Sort { get; set; }
        public string AdditionalSetting { get; set; }
        public string Translation { get; set; }
        public AdminFunctionStatusV2Enum Status { get; set; }
    }

    public class AdminPermissionDtoV2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public string Translation { get; set; }
        public AdminPermissionStatusV2Enum Status { get; set; }
    }
    public class AdditionalSetting
    {
        public IEnumerable<int> Permission { get; set; }
    }
    public class AdminRole
    {
        public int MenuId { get; set; }
        public string Permission { get; set; }
    }
}
