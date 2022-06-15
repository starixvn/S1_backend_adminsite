using BackendSite.Service.Model.enums;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class AdminMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int HeirId { get; set; }
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
