using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class AdminModuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Layer { get; set; }
        public int HeirId { get; set; }
        public int Sort { get; set; }
        public string Translation { get; set; }
        public AdminModuleStatusEnum Status { get; set; }
    }
}
