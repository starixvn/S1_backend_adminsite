using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class AdminPermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public string Translation { get; set; }
        public AdminPermissionStatusEnum Status { get; set; }
    }
}
