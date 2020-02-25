using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class AdminGroup
    {
        public int? GroupSEQ { get; set; }
        public string GroupName { get; set; }
        public string Remark { get; set; }
    }

    public class GroupWithPermission : AdminGroup
    {
        public IEnumerable<FunctionPermission> Permission { get; set; }
    }

    public class FunctionPermission
    {
        public string FunctionKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] SubFunctionKeys { get; set; }
    }
}
