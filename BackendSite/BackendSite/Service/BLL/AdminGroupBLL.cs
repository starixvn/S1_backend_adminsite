using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class AdminGroupBLL
    {
        private readonly AdminGroupService _adminGroupService;

        public AdminGroupBLL(IServiceProvider provider)
        {
            _adminGroupService = provider.GetRequiredService<AdminGroupService>();
        }

        public IEnumerable<AdminGroup> GetGroupList(int pageNumber,
            int pageSize,
            out int totalPage,
            out int rowCount)
        {
            return _adminGroupService.GetGroupList((Math.Abs(pageNumber) - 1) * pageSize,
               Math.Abs(pageSize),
               out totalPage,
               out rowCount);
        }

        public IEnumerable<AdminGroup> GetGroupByIdOrAll(int groupId,bool isAll)
        {
            return _adminGroupService.GetGroupByIdOrAll(groupId,isAll);
        }

        public bool CheckGroupIsExists(string GroupName, int? SEQ)
        {
            var all_groups = _adminGroupService.GetGroupList(0, 1000, out _, out _);
            return all_groups.Any(x => x.GroupSEQ != SEQ && string.Equals(x.GroupName, GroupName, StringComparison.OrdinalIgnoreCase));
        }

        public void AddGroup(GroupWithPermission GroupInfo)
        {
            _adminGroupService.AddGroup(GroupInfo);
        }

        public void DeleteGroup(int GroupSEQ)
        {
            _adminGroupService.DeleteGroup(GroupSEQ);
        }

        public void UpdateGroup(GroupWithPermission GroupInfo)
        {
            _adminGroupService.UpdateGroup(GroupInfo);
        }
    }
}
