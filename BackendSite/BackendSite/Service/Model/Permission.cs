using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
}
