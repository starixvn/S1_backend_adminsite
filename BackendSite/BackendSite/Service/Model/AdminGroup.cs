using Newtonsoft.Json;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class AdminGroup
    {
        public int? GroupSEQ { get; set; }
        public string GroupName { get; set; }
        public string Remark { get; set; }
    }

    public class AdminGroupModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Remark { get; set; }
    }

    public class GroupWithPermission : AdminGroup
    {
        public IEnumerable<FunctionPermission> Permission { get; set; }
    }

    public class FunctionPermission
    {
        public string FuncKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] SubFuncKeys { get; set; }
    }
}
