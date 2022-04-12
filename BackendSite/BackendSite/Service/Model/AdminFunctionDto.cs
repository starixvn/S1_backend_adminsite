using BackendSite.Service.Model.enums;
using System.Collections;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class AdminFunctionDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public string AdditionalSetting { get; set; }
        public string Translation { get; set; }
        public AdminFunctionStatusEnum Status { get; set; }
    }

    public class AdditionalSettingModel
    {
        public IEnumerable<int> Permission { get; set; }
    }

}
